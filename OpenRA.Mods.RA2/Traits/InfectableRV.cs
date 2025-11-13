#region Copyright & License Information
/*
 * Copyright 2015- OpenRA.Mods.AS Developers (see AUTHORS)
 * This file is a part of a third-party plugin for OpenRA, which is
 * free software. It is made available to you under the terms of the
 * GNU General Public License as published by the Free Software
 * Foundation. For more information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using OpenRA.Mods.AS.Traits;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.RA2.Traits
{
	[Desc("Handle infection by infector units.")]
	public class InfectableRVInfo : ConditionalTraitInfo, Requires<HealthInfo>
	{
		[Desc("Damage types that removes the infector.")]
		public readonly BitSet<DamageType> RemoveInfectorDamageTypes = default;

		[Desc("Damage types that kills the infector.")]
		public readonly BitSet<DamageType> KillInfectorDamageTypes = default;

		[Desc("Teleport types that removes the infector.")]
		public readonly HashSet<string> RemoveInfectorTeleportTypes = default;

		[GrantedConditionReference]
		[Desc("The condition to grant to self while infected by any actor.")]
		public readonly string InfectedCondition = null;

		[GrantedConditionReference]
		[Desc("Condition granted when being infected by another actor.")]
		public readonly string BeingInfectedCondition = null;

		[Desc("Conditions to grant when infected by specified actors.",
			"A dictionary of [actor id]: [condition].")]
		public readonly Dictionary<string, string> InfectedByConditions = new();

		[GrantedConditionReference]
		public IEnumerable<string> LinterConditions { get { return InfectedByConditions.Values; } }

		public override object Create(ActorInitializer init) { return new InfectableRV(init.Self, this); }
	}

	public class InfectableRV : ConditionalTrait<InfectableRVInfo>, ISync, ITick, INotifyDamage, INotifyKilled, IOnSuccessfulTeleportRA2
	{
		readonly Health health;
		readonly Actor self;

		public Tuple<Actor, AttackInfectRV, AttackInfectRVInfo> Infector;
		public int[] FirepowerMultipliers = Array.Empty<int>();

		[Sync]
		public int Ticks;

		int beingInfectedToken = Actor.InvalidConditionToken;
		Actor enteringInfector;
		int infectedToken = Actor.InvalidConditionToken;
		int infectedByToken = Actor.InvalidConditionToken;

		int dealtDamage = 0;
		int suppressionCount = 0;

		bool killInfectorOnDeath = false;

		public InfectableRV(Actor self, InfectableRVInfo info)
			: base(info)
		{
			this.self = self;
			health = self.Trait<Health>();
		}

		public bool TryStartInfecting(Actor self, Actor infector)
		{
			if (infector != null)
			{
				if (enteringInfector == null)
				{
					enteringInfector = infector;

					if (beingInfectedToken == Actor.InvalidConditionToken && !string.IsNullOrEmpty(Info.BeingInfectedCondition))
						beingInfectedToken = self.GrantCondition(Info.BeingInfectedCondition);

					return true;
				}
			}

			return false;
		}

		public void GrantCondition(Actor self)
		{
			if (infectedToken == Actor.InvalidConditionToken && !string.IsNullOrEmpty(Info.InfectedCondition))
				infectedToken = self.GrantCondition(Info.InfectedCondition);

			if (Info.InfectedByConditions.TryGetValue(Infector.Item1.Info.Name, out var infectedByCondition))
				infectedByToken = self.GrantCondition(infectedByCondition);
		}

		public void RevokeCondition(Actor self, Actor infector = null)
		{
			if (infector != null)
			{
				if (enteringInfector == infector)
				{
					enteringInfector = null;

					if (beingInfectedToken != Actor.InvalidConditionToken)
						beingInfectedToken = self.RevokeCondition(beingInfectedToken);
				}
			}
			else
			{
				if (infectedToken != Actor.InvalidConditionToken)
					infectedToken = self.RevokeCondition(infectedToken);

				if (infectedByToken != Actor.InvalidConditionToken)
					infectedByToken = self.RevokeCondition(infectedByToken);
			}
		}

		void RemoveInfector(Actor self, WPos spawnLoc, bool kill, AttackInfo e)
		{
			if (Infector != null && !Infector.Item1.IsDead)
			{
				Infector.Item1.TraitOrDefault<IPositionable>().SetPosition(Infector.Item1, spawnLoc);
				self.World.AddFrameEndTask(w =>
				{
					if (Infector == null || Infector.Item1.IsDead)
						return;

					w.Add(Infector.Item1);

					if (kill)
					{
						if (e != null)
							Infector.Item1.Kill(e.Attacker, e.Damage.DamageTypes);
						else
							Infector.Item1.Kill(self);
					}
					else
						Infector.Item1.QueueActivity(false, new Nudge(Infector.Item1));

					RevokeCondition(self);
					Infector = null;
					FirepowerMultipliers = Array.Empty<int>();
					dealtDamage = 0;
					suppressionCount = 0;
					killInfectorOnDeath = false;
				});
			}
		}

		void INotifyDamage.Damaged(Actor self, AttackInfo e)
		{
			// Early exit: no infector present
			if (Infector == null)
				return;

			// Cache commonly accessed Infector properties to avoid repeated Tuple access
			var infectorActor = Infector.Item1;
			var infectorInfo = Infector.Item3;

			if (e.Damage.DamageTypes.Overlaps(Info.KillInfectorDamageTypes))
				RemoveInfector(self, self.CenterPosition, true, e);
			else if (e.Damage.DamageTypes.Overlaps(Info.RemoveInfectorDamageTypes))
				RemoveInfector(self, self.CenterPosition, false, e);
			else if (e.Attacker != infectorActor && e.Damage.DamageTypes.Overlaps(infectorInfo.SuppressionDamageType))
			{
				// Check thresholds efficiently - short-circuit evaluation
				var suppressionDamageThreshold = infectorInfo.SuppressionDamageThreshold;
				if (suppressionDamageThreshold > 0 && e.Damage.Value > suppressionDamageThreshold)
					killInfectorOnDeath = true;

				dealtDamage += e.Damage.Value;
				var suppressionSumThreshold = infectorInfo.SuppressionSumThreshold;
				if (suppressionSumThreshold > 0 && dealtDamage > suppressionSumThreshold)
					killInfectorOnDeath = true;

				suppressionCount++;
				var suppressionCountThreshold = infectorInfo.SuppressionCountThreshold;
				if (suppressionCountThreshold > 0 && suppressionCount > suppressionCountThreshold)
					killInfectorOnDeath = true;
			}
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			// Early exit: no infector present
			if (Infector == null)
				return;

			var shdt = Infector.Item3.SurviveHostDamageTypes;
			var kill = killInfectorOnDeath || (!shdt.IsEmpty && !shdt.Overlaps(e.Damage.DamageTypes));
			RemoveInfector(self, self.CenterPosition, kill, e);
		}

		void ITick.Tick(Actor self)
		{
			// Early exit: trait disabled or no infector
			if (IsTraitDisabled || Infector == null)
				return;

			if (--Ticks < 0)
			{
				// Cache Infector properties to avoid repeated Tuple field access
				var infectorActor = Infector.Item1;
				var infectorInfo = Infector.Item3;

				var damage = Util.ApplyPercentageModifiers(infectorInfo.Damage, FirepowerMultipliers);
				health.InflictDamage(self, infectorActor, new Damage(damage, infectorInfo.DamageTypes), false);

				Ticks = infectorInfo.DamageInterval;
			}
		}

		void IOnSuccessfulTeleportRA2.OnSuccessfulTeleport(string type, WPos oldPos, WPos newPos)
		{
			// Early exit: no removal teleport types configured or no infector
			if (Info.RemoveInfectorTeleportTypes == null || Infector == null)
				return;

			if (Info.RemoveInfectorTeleportTypes.Contains(type))
				RemoveInfector(self, oldPos, false, null);
		}
	}
}
