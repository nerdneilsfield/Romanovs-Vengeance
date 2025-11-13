#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.RA2.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.RA2.Activities
{
	public class HeliDeployForGrantedCondition : Activity
	{
		readonly World w;
		readonly Aircraft aircraft;
		readonly HeliGrantConditionOnDeploy deploy;
		readonly bool canTurn;
		readonly bool moving;

		public HeliDeployForGrantedCondition(Actor self, HeliGrantConditionOnDeploy deploy, bool moving = false)
		{
			w = self.World;
			this.deploy = deploy;
			this.moving = moving;
			aircraft = self.Trait<Aircraft>();
			canTurn = self.Info.HasTraitInfo<IFacingInfo>();
		}

		protected override void OnFirstRun(Actor self)
		{
			var currentCell = w.Map.CellContaining(self.CenterPosition);

			if (!aircraft.CanLand(currentCell))
			{
				var landingPos = FindLandingPosition(self, currentCell, deploy.Info.LandingSearchRadius);

				if (landingPos.HasValue)
				{
					var cell = w.Map.CellContaining(landingPos.Value);
					QueueChild(new Fly(self, Target.FromCell(w, cell)));
				}
			}

			// Turn to the required facing.
			if (deploy.DeployState == DeployState.Undeployed && deploy.Info.Facing != -1 && canTurn && !moving)
				QueueChild(new Turn(self, WAngle.FromFacing(deploy.Info.Facing)));

			QueueChild(new Land(self));
		}

		public override bool Tick(Actor self)
		{
			if (IsCanceling || (deploy.DeployState != DeployState.Deployed && moving))
			{
				// Ensure all child activities are properly cancelled and can clean up their state
				Cancel(self);
				return true;
			}

			QueueChild(new HeliDeployInner(self, deploy));
			return true;
		}

		protected override void OnActorDispose(Actor self)
		{
			// Defensive cleanup: Ensure aircraft influence is removed if activity is aborted
			// This catches edge cases where Cancel() might not have been called or child activities
			// were interrupted before they could clean up properly
			if (aircraft.HasInfluence())
				aircraft.RemoveInfluence();

			base.OnActorDispose(self);
		}

		WPos? FindLandingPosition(Actor self, CPos origin, int searchRadius)
		{
			var landingPos = FindLandingPositionInRadius(self, origin, searchRadius);
			if (landingPos.HasValue)
				return landingPos;

			if (searchRadius > 0)
				return FindLandingPositionInRadius(self, origin, -1);

			return null;
		}

		WPos? FindLandingPositionInRadius(Actor self, CPos origin, int radius)
		{
			var cells = radius > 0
				? w.Map.FindTilesInAnnulus(origin, 1, radius)
				: w.Map.AllCells;

			var positions = cells
				.Where(c => aircraft.CanLand(c))
				.Select(c => w.Map.CenterOfCell(c))
				.ToList();

			if (positions.Count == 0)
				return null;

			return positions.ClosestToIgnoringPath(self.CenterPosition);
		}
	}

	public class HeliDeployInner : Activity
	{
		readonly HeliGrantConditionOnDeploy deployment;
		bool initiated;

		public HeliDeployInner(Actor self, HeliGrantConditionOnDeploy deployment)
		{
			this.deployment = deployment;

			// Once deployment animation starts, the animation must finish.
			IsInterruptible = false;
		}

		public override bool Tick(Actor self)
		{
			// Wait for deployment
			if (deployment.DeployState == DeployState.Deploying || deployment.DeployState == DeployState.Undeploying)
				return false;

			if (initiated)
				return true;

			if (deployment.DeployState == DeployState.Undeployed)
				deployment.Deploy();
			else
				deployment.Undeploy();

			initiated = true;
			return false;
		}
	}
}
