# Romanov's Vengeance - Complete Codebase Guide

A comprehensive reference for understanding and navigating the Romanov's Vengeance OpenRA mod codebase.

---

## Quick Start

1. **First Time Reading?** Start with section 1 below
2. **Need to Add Something?** Jump to section 5 (Development Checklists)
3. **Looking for Specific File?** See section 3 (File Organization)
4. **Want to Understand Architecture?** See section 2 (How It Works)

---

## 1. Project Overview

**Project**: Romanov's Vengeance - Third-party OpenRA mod based on Red Alert 2  
**Language**: C# (.NET) for logic, YAML for configuration  
**Build System**: Makefile (Makefile for Unix, make.ps1 for Windows)  
**Main Executable**: `./launch-game.sh` (Linux/Mac) or `launch-game.cmd` (Windows)

### Statistics
- C# Code: 23 files, ~2000 lines
- YAML Configuration: 45+ files, ~51,795 lines  
- Total Traits: 10 core custom traits
- Total Activities: 4 state machines
- Total Warheads: 5 custom effects
- Total Factions: 4 (Allied, Soviet, Yuri, Bakuvian)
- Total Terrains: 7 tileset variations

### Custom Features
- Infection system (virus-based units)
- Mirage units (decoy mechanics)
- Helicopter deployment system
- Temporal damage mechanics
- Resource theft mechanics
- Garrison system (custom interfaces)

---

## 2. Architecture Overview

### Component Layers

```
APPLICATION (Game)
    ↑
OpenRA Engine (./engine/)
    [Core game loop, rendering, audio, multiplayer]
    ├─ OpenRA.Game.csproj
    ├─ OpenRA.Mods.Common [shared traits]
    ├─ OpenRA.Mods.Cnc [C&C specific]
    └─ OpenRA.Mods.AS [additional support]
    ↑
Custom Mod Code (./OpenRA.Mods.RA2/)
    [Game-specific mechanics via traits, activities, warheads]
    ↑
YAML Configuration (./mods/rv/)
    [Actor definitions, unit configurations, weapon balance]
```

### Key Concepts

#### Traits
Composable behavioral components that define how actors behave.

**Example Pattern:**
```yaml
soviet_tank:
  Inherits: ^Vehicle        # Base traits (movement, health, etc)
  Armament:                 # Attack capability
    Weapon: tank_cannon
  Turreted:                 # Rotating turret
    TurnSpeed: 40
```

**In C#:**
```csharp
public class CustomTraitInfo : ConditionalTraitInfo
{
    public override object Create(ActorInitializer init) => new CustomTrait(init.Self, this);
}

public class CustomTrait : ConditionalTrait<CustomTraitInfo>
{
    // Implement trait logic
}
```

#### Activities
State machines that control actor actions (movement, attacking, deploying).

**Example:**
```csharp
public class HeliDeployForGrantedCondition : Activity
{
    protected override void OnFirstRun(Actor self)
    {
        QueueChild(new Land(self));
        QueueChild(new Turn(self, targetFacing));
    }
    
    public override bool Tick(Actor self)
    {
        // Process activity each game tick
    }
}
```

#### Warheads
Effects applied when weapons hit targets.

**Example:**
```csharp
public class StealResourceWarhead : Warhead
{
    public override void DoImpact(in Target target, WarheadArgs args)
    {
        // Execute on impact
        // Can damage, create effects, modify game state
    }
}
```

#### Conditions
Named boolean states that control trait behavior reactively.

**Example Pattern:**
```yaml
Armament@elite:
  Weapon: elite_gun
  RequiresCondition: rank-elite    # Only when condition active
```

#### Sequences
Animation and sprite definitions.

**Example:**
```yaml
tank:
  AttackTurreted:
    1: tank-trs 1
    2: tank-trs 2
```

---

## 3. File Organization

### Root Directory Structure

```
/home/dengqi/Source/langs/csharp/Romanovs-Vengeance/
├── OpenRA.Mods.RA2/              [C# Custom Code]
├── mods/rv/                       [YAML Configuration]
├── engine/                        [OpenRA Engine (auto-fetched)]
├── packaging/                     [Build Distribution]
├── OpenRA.Mods.RA2.sln           [Visual Studio Solution]
├── mod.config                     [Core Configuration]
├── Makefile / make.ps1            [Build System]
├── fetch-engine.sh                [Engine Setup]
├── launch-game.sh / .cmd          [Game Launcher]
└── CLAUDE.md                      [Official Guide - READ THIS FIRST]
```

### C# Code Organization

```
OpenRA.Mods.RA2/
├── TraitsInterfaces.cs            [Custom trait interfaces - 27 lines, SIMPLE]
├── Traits/ (10 files)
│   ├── Mirage.cs                  [Complex trait - 150+ lines]
│   ├── InfectableRV.cs            [Infection system]
│   ├── AttackInfectRV.cs          [Infection attack]
│   ├── BallisticMissileOld.cs     [Legacy missile]
│   ├── MissileSpawner*.cs         [Multi-part missiles]
│   ├── WithAcceptDeliveredCashSound.cs [Simple - 30 lines, EXCELLENT START]
│   ├── AffectedByTemporal.cs
│   ├── InfectorOld.cs
│   ├── InfectableOld.cs
│   ├── Conditions/                [Conditional traits]
│   │   ├── HeliGrantConditionOnDeploy.cs [Medium - 100+ lines]
│   │   └── GrantConditionOnOwnerLost.cs
│   ├── Render/                    [Visual traits]
│   │   ├── WithMirageSpriteBody.cs
│   │   ├── WithCargoBuilding.cs
│   │   ├── WithIdleRepairOverlay.cs
│   │   └── WithSupportPowerChargedOverlay.cs
│   └── Sound/                     [Audio traits]
│       ├── CaptureSound.cs
│       └── SoundAnnouncement.cs
├── Activities/ (4 files)
│   ├── HeliDeployForGrantedCondition.cs [Deploy state machine - 80+ lines]
│   ├── InfectRV.cs
│   ├── BallisticMissileFlyOld.cs
│   └── InfectOld.cs
├── Warheads/ (5 files)
│   ├── StealResourceWarhead.cs    [Simple - 52 lines, EXCELLENT EXAMPLE]
│   ├── TemporalDamageWarhead.cs
│   ├── SpawnActorOrWeaponWarhead.cs
│   ├── SpawnBuildingOrWeaponWarhead.cs
│   └── LegacySpreadWarhead.cs
├── PaletteEffects/
│   └── ColorAlphaFlashPaletteEffect.cs
├── Widgets/
│   └── Logic/
├── UtilityCommands/
│   └── ImportRA2MapCommand.cs
└── FileSystem/
```

### YAML Configuration Organization

```
mods/rv/
├── mod.yaml                       [MASTER CONFIG - 415 lines, START HERE]
├── rules/ (45 files, ~51,795 lines)
│   ├── defaults.yaml              [Base trait templates - ESSENTIAL]
│   ├── player.yaml                [Player config]
│   ├── world.yaml                 [World config]
│   ├── palettes.yaml              [Color palettes]
│   ├── proxy-actors.yaml          [Engine proxies]
│   ├── misc.yaml                  [Misc actors]
│   ├── {faction}-vehicles.yaml (4 files)
│   ├── {faction}-infantry.yaml (4 files)
│   ├── {faction}-structures.yaml (4 files)
│   ├── {faction}-naval.yaml (4 files)
│   ├── aircraft.yaml              [Helicopters]
│   ├── civilian-*.yaml (6 files)
│   ├── cpowers.yaml               [Superweapons]
│   ├── upgrades.yaml              [Tech tree]
│   ├── bridges.yaml, trees.yaml, animals.yaml, arrakis.yaml
│   ├── ai-only-units.yaml
│   ├── campaign-rules.yaml
│   ├── debug-structures.yaml
│   ├── tech-structures.yaml
│   ├── old-vehicles.yaml
│   ├── default-vehicles.yaml
│   ├── default-structures.yaml
│   └── default-naval.yaml
├── sequences/ (23 files)
│   ├── vehicles.yaml
│   ├── {faction}-infantry.yaml (4 files)
│   ├── {faction}-structures.yaml (4 files)
│   ├── aircraft.yaml, civilians.yaml, civilian-*.yaml, animals.yaml, trees.yaml, etc.
│   ├── voxels.yaml                [3D models]
│   ├── cpowers.yaml, upgrades.yaml, defaults.yaml, misc.yaml
│   └── arrakis.yaml
├── weapons/ (14 files)
│   ├── defaults.yaml              [Weapon templates - COMPLEX EXAMPLE]
│   ├── bullets.yaml, missiles.yaml, melee.yaml
│   ├── flaks.yaml, gatling.yaml, mgs.yaml, zaps.yaml
│   ├── gravitybombs.yaml, ivanbombs.yaml, ifvweapons.yaml
│   ├── explosions.yaml, debris.yaml, misc.yaml
│   └── [All weapon/warhead definitions]
├── chrome/                        [UI Layouts]
├── audio/                         [Voices, Music, Sounds]
├── tilesets/ (7 files)            [Terrain definitions]
├── bits/                          [Game Assets]
│   ├── terrain/                   [Terrain sprites]
│   ├── structures/                [Building sprites]
│   ├── vehicles/, infantry/       [Unit sprites]
│   ├── animations/, projectiles/  [Effects]
│   ├── cameos/                    [Unit portraits]
│   └── audio/                     [Sound files]
├── fluent/                        [Localization strings]
├── scripts/                       [Lua scripting]
└── maps/                          [Game maps]
```

---

## 4. Recommended Reading Order

### For Understanding Architecture (1-2 hours)
1. `/mods/rv/mod.yaml` (415 lines) - Overview of all components
2. `/OpenRA.Mods.RA2/TraitsInterfaces.cs` (27 lines) - Custom interfaces
3. `/mods/rv/rules/defaults.yaml` - Base trait templates
4. `/OpenRA.Mods.RA2.sln` - Project structure

### For Understanding Traits (2-3 hours)
1. `/mods/rv/rules/soviet-vehicles.yaml` - Real-world example (harvester unit)
2. `/OpenRA.Mods.RA2/Traits/WithAcceptDeliveredCashSound.cs` - Simple trait (30 lines)
3. `/OpenRA.Mods.RA2/Traits/Conditions/HeliGrantConditionOnDeploy.cs` - Medium complexity (100+ lines)
4. `/OpenRA.Mods.RA2/Traits/Mirage.cs` - Complex trait (150+ lines)

### For Understanding Warheads (1 hour)
1. `/OpenRA.Mods.RA2/Warheads/StealResourceWarhead.cs` - Simple example (52 lines)
2. `/mods/rv/weapons/defaults.yaml` - Weapon/warhead templates
3. `/mods/rv/weapons/flaks.yaml` - Real-world weapon example

### For Understanding Activities (1 hour)
1. `/OpenRA.Mods.RA2/Traits/Conditions/HeliGrantConditionOnDeploy.cs` - Trait with activity
2. `/OpenRA.Mods.RA2/Activities/HeliDeployForGrantedCondition.cs` - Activity implementation

### For Understanding Game Rules (2 hours)
1. `/mods/rv/rules/defaults.yaml` - Templates
2. `/mods/rv/rules/soviet-vehicles.yaml` - Vehicle definitions
3. `/mods/rv/rules/allied-structures.yaml` - Building definitions
4. `/mods/rv/rules/cpowers.yaml` - Complex system (superweapons)

---

## 5. Development Checklists

### Adding a New Unit

- [ ] Edit `/mods/rv/rules/{faction}-{type}.yaml`
  - [ ] Choose faction: allied, soviet, yuri, or bakuvian
  - [ ] Choose type: vehicles, infantry, structures, naval, aircraft
  - [ ] Add actor definition with traits
- [ ] Edit `/mods/rv/sequences/{type}.yaml`
  - [ ] Add sprite sequence animations
- [ ] Run `make test` to validate YAML
- [ ] Build with `make`
- [ ] Test with `./launch-game.sh`

### Adding a New Trait

- [ ] Create `/OpenRA.Mods.RA2/Traits/{TraitName}.cs`
  - [ ] Inherit from `ConditionalTrait<TraitNameInfo>` or appropriate base
  - [ ] Create matching `TraitNameInfo` class
  - [ ] Implement required methods
- [ ] Build with `make`
- [ ] Add to YAML rules files: `TraitName:`
- [ ] Run `make test` to validate
- [ ] Test with `./launch-game.sh`

### Adding a New Activity

- [ ] Create `/OpenRA.Mods.RA2/Activities/{ActivityName}.cs`
  - [ ] Inherit from `Activity`
  - [ ] Implement `OnFirstRun(Actor self)` for initialization
  - [ ] Implement `Tick(Actor self)` for state machine
- [ ] Build with `make`
- [ ] Use from trait: `QueueActivity(new ActivityName(self))`
- [ ] Test with `./launch-game.sh`

### Adding a New Warhead

- [ ] Create `/OpenRA.Mods.RA2/Warheads/{WarheadName}.cs`
  - [ ] Inherit from `Warhead` or `WarheadAS`
  - [ ] Implement `DoImpact(in Target target, WarheadArgs args)`
- [ ] Build with `make`
- [ ] Add to `/mods/rv/weapons/{file}.yaml`
  - [ ] Define weapon with: `Warhead@Name: WarheadName`
- [ ] Test with `./launch-game.sh`

### Adding a New Weapon

- [ ] Edit `/mods/rv/weapons/{file}.yaml`
  - [ ] Define projectile type (speed, trajectory, etc.)
  - [ ] Add warheads (damage, effects, etc.)
- [ ] Reference in actor: `Weapon: weaponname`
- [ ] Run `make test`
- [ ] Test with `./launch-game.sh`

---

## 6. Key Patterns & Examples

### Pattern 1: Multiple Inheritance
```yaml
actor:
  Inherits: ^BaseTemplate
  Inherits@EXPERIENCE: ^GainsExperience
  Inherits@BUNKER: ^Bunkerable
```

### Pattern 2: Conditional Traits
```yaml
Armament@elite:
  Weapon: better_gun
  RequiresCondition: rank-elite

Armament@normal:
  Weapon: normal_gun
  RequiresCondition: !rank-elite
```

### Pattern 3: Warhead Chaining
```yaml
Weapon: missile
  Warhead@1Dam: SpreadDamage
    Damage: 2000
  Warhead@2Eff: CreateEffect
    Explosions: boom
  Warhead@3Sound: CreateEffect
    ImpactSounds: impact.wav
```

### Pattern 4: Trait-Activity Interaction
```csharp
// In Trait
public void OnDeploy()
{
    self.QueueActivity(new DeployActivity(self));
}

// In Activity
protected override void OnFirstRun(Actor self)
{
    QueueChild(new Land(self));
    QueueChild(new Turn(self, facing));
}
```

---

## 7. Common Commands

```bash
# Build everything
make

# Clean build artifacts
make clean

# Validate YAML syntax
make test

# Check C# code style (StyleCop)
make check

# Set mod version
make version VERSION="x.y.z"

# Run the game
./launch-game.sh

# Run dedicated server
./launch-dedicated.sh

# Utility commands
./utility.sh --check-yaml
./utility.sh --check-explicit-interfaces
./utility.sh --check-conditional-trait-interface-overrides
```

---

## 8. Entry Point Examples

### Simple Entry: WithAcceptDeliveredCashSound.cs
**File**: `/home/dengqi/Source/langs/csharp/Romanovs-Vengeance/OpenRA.Mods.RA2/Traits/WithAcceptDeliveredCashSound.cs`
- Lines: ~30
- Complexity: Simple
- Purpose: Play sound when cash is delivered
- Pattern: Event subscription, dependency injection

### Simple Entry: StealResourceWarhead.cs
**File**: `/home/dengqi/Source/langs/csharp/Romanovs-Vengeance/OpenRA.Mods.RA2/Warheads/StealResourceWarhead.cs`
- Lines: 52
- Complexity: Simple
- Purpose: Steal cash on impact
- Pattern: Warhead implementation, floating text effects

### Medium Entry: HeliGrantConditionOnDeploy.cs
**File**: `/home/dengqi/Source/langs/csharp/Romanovs-Vengeance/OpenRA.Mods.RA2/Traits/Conditions/HeliGrantConditionOnDeploy.cs`
- Lines: 100+
- Complexity: Medium
- Purpose: Deploy/undeploy mechanics with conditions
- Pattern: Complex trait with multiple options

### Complex Entry: Mirage.cs
**File**: `/home/dengqi/Source/langs/csharp/Romanovs-Vengeance/OpenRA.Mods.RA2/Traits/Mirage.cs`
- Lines: 150+
- Complexity: Complex
- Purpose: Decoy unit mechanics (visual deception)
- Pattern: Multiple sub-traits, condition system

### Complex Entry: soviet-vehicles.yaml
**File**: `/home/dengqi/Source/langs/csharp/Romanovs-Vengeance/mods/rv/rules/soviet-vehicles.yaml`
- Section: Harvester (harv)
- Complexity: Complex real-world example
- Purpose: Complete unit definition with multiple weapon variants
- Pattern: Multiple inheritance, conditional traits

---

## 9. Important Files

| File | Purpose | Lines | Importance |
|------|---------|-------|-----------|
| /mods/rv/mod.yaml | Master configuration | 415 | Critical |
| /mods/rv/rules/defaults.yaml | Base templates | 200+ | Critical |
| /OpenRA.Mods.RA2/TraitsInterfaces.cs | Custom interfaces | 27 | Important |
| /OpenRA.Mods.RA2/Traits/Mirage.cs | Complex trait | 150+ | Reference |
| /OpenRA.Mods.RA2/Warheads/StealResourceWarhead.cs | Simple warhead | 52 | Learning |
| /mods/rv/rules/soviet-vehicles.yaml | Unit examples | 500+ | Reference |
| /mods/rv/weapons/defaults.yaml | Weapon templates | 300+ | Reference |
| /CLAUDE.md | Official guide | - | Critical |

---

## 10. External Resources

- **OpenRA Documentation**: https://github.com/OpenRA/OpenRA/wiki
- **OpenRA Mod SDK**: https://github.com/OpenRA/ModSDK
- **Project Documentation**: `/CLAUDE.md` in this directory

---

## 11. Troubleshooting

### YAML Validation Fails
```bash
make test
```
Check error messages - most common are:
- Incorrect indentation (use spaces, not tabs)
- Missing colon after property names
- Undefined trait or weapon references

### Build Fails
```bash
make clean
make
```
Ensure engine is present:
```bash
./fetch-engine.sh
```

### Game Won't Start
- Verify all YAML files are valid: `make test`
- Check C# compilation: `make` should complete without errors
- Ensure mod.yaml references are correct

---

## 12. Project Statistics Summary

**Codebase Size:**
- C# Code: 23 files, ~2000 lines
- YAML Configuration: 45+ files, ~51,795 lines
- Asset Files: Hundreds (sprites, audio)
- Total Project: ~5MB+ with assets

**Organization:**
- Factions: 4 (Allied, Soviet, Yuri, Bakuvian)
- Unit Types: 5+ per faction (vehicles, infantry, structures, naval, air)
- Terrains: 7 variations
- Weapons: 14 categories
- Game Systems: 20+ (upgrades, powers, etc.)

**Custom Content:**
- Traits: 10 core custom
- Activities: 4 state machines
- Warheads: 5 custom effects
- Interfaces: 4 custom trait interfaces

---

Generated for Romanov's Vengeance OpenRA Mod  
Location: `/home/dengqi/Source/langs/csharp/Romanovs-Vengeance/CODEBASE_GUIDE.md`
