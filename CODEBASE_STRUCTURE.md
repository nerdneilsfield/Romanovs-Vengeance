# Romanov's Vengeance Codebase Structure Guide

Complete reference for navigating and understanding the RV mod codebase.

## Quick Navigation

### I Need to Understand...

**The overall structure**
- Start: `/mods/rv/mod.yaml` (main configuration)
- Then: `/OpenRA.Mods.RA2/TraitsInterfaces.cs` (custom interfaces)

**How to create a new unit**
- 1. Edit: `/mods/rv/rules/{faction}-{type}.yaml` (add actor definition)
- 2. Edit: `/mods/rv/sequences/{type}.yaml` (add sprites)
- 3. Run: `make test` (validate)

**How traits work**
- Theory: See `/OpenRA.Mods.RA2/TraitsInterfaces.cs`
- Simple example: `/OpenRA.Mods.RA2/Traits/WithAcceptDeliveredCashSound.cs`
- Complex example: `/OpenRA.Mods.RA2/Traits/Mirage.cs`

**How weapons/damage work**
- See: `/mods/rv/weapons/defaults.yaml` (weapon templates)
- Warhead code: `/OpenRA.Mods.RA2/Warheads/StealResourceWarhead.cs`

**Helicopter deployment**
- Code: `/OpenRA.Mods.RA2/Traits/Conditions/HeliGrantConditionOnDeploy.cs`
- Activity: `/OpenRA.Mods.RA2/Activities/HeliDeployForGrantedCondition.cs`

---

## Directory Map

```
OpenRA.Mods.RA2/              C# Custom Code (23 files)
  ├─ Traits/                  (10 gameplay mechanics)
  │  ├─ Conditions/           (2 conditional traits)
  │  ├─ Render/               (4 visual traits)
  │  └─ Sound/                (2 audio traits)
  ├─ Activities/              (4 state machines)
  ├─ Warheads/                (5 impact effects)
  ├─ PaletteEffects/          (1 screen effects)
  ├─ Widgets/                 (UI components)
  ├─ UtilityCommands/         (1 dev tool)
  └─ TraitsInterfaces.cs      (mod-specific interfaces)

mods/rv/                      YAML Configuration (~52K lines)
  ├─ rules/                   (45 actor definition files)
  │  ├─ defaults.yaml         (base trait templates)
  │  ├─ {faction}-*.yaml      (unit definitions)
  │  ├─ cpowers.yaml          (superweapons)
  │  └─ [15 more organization files]
  ├─ sequences/               (23 animation files)
  ├─ weapons/                 (14 weapon definition files)
  ├─ chrome/                  (UI layout files)
  ├─ audio/                   (voices, music, sound)
  ├─ tilesets/                (7 terrain definitions)
  ├─ bits/                    (game assets)
  ├─ fluent/                  (localization)
  ├─ scripts/                 (Lua scripting)
  ├─ maps/                    (game maps)
  └─ mod.yaml                 (main configuration file)

engine/                       OpenRA Engine Fork (auto-fetched)
```

---

## File Quick Reference

### C# Entry Points

| File | Purpose | Lines | Complexity |
|------|---------|-------|-----------|
| TraitsInterfaces.cs | Custom interfaces | 27 | Simple |
| WithAcceptDeliveredCashSound.cs | Play sound on event | ~30 | Simple |
| StealResourceWarhead.cs | Custom warhead | 52 | Simple |
| HeliGrantConditionOnDeploy.cs | Deploy condition | 100+ | Medium |
| HeliDeployForGrantedCondition.cs | Deploy activity | 80+ | Medium |
| Mirage.cs | Complex trait system | 150+ | Complex |

### YAML Entry Points

| File | Purpose | Lines | Scope |
|------|---------|-------|-------|
| mod.yaml | Main configuration | 415 | Entire mod |
| defaults.yaml | Base trait templates | 200+ | All actors |
| soviet-vehicles.yaml | Vehicle definitions | 500+ | Soviet units |
| cpowers.yaml | Superweapons | 300+ | Game systems |
| weapons/defaults.yaml | Weapon templates | 300+ | All weapons |

---

## Architecture Patterns

### 1. Trait Composition
```yaml
actor:
  Inherits: ^BaseTemplate
  Inherits@TAG1: ^MoreTraits
  Inherits@TAG2: ^OtherTraits
  CustomTrait:
    Property: value
```

### 2. Conditional Logic
```yaml
Armament@elite:
  Weapon: bettergun
  RequiresCondition: rank-elite     # Only when condition active
```

### 3. Multiple Warheads
```yaml
Weapon: missile
  Warhead@1: SpreadDamage           # Primary effect
    Damage: 100
  Warhead@2: CreateEffect           # Visual effect
    Explosions: boom
```

### 4. Trait-Activity Interaction
C# trait initiates activity:
```csharp
QueueActivity(new CustomActivity(self, this));
```
Activity reads trait state and conditions.

---

## Key Traits by Category

### Rendering
- WithMirageSpriteBody
- WithCargoBuilding
- WithIdleRepairOverlay
- WithSupportPowerChargedOverlay

### Behavior
- HeliGrantConditionOnDeploy
- GrantConditionOnOwnerLost
- Mirage

### Infection System
- InfectableRV
- AttackInfectRV
- InfectRV (activity)

### Audio
- CaptureSound
- SoundAnnouncement
- WithAcceptDeliveredCashSound

### Weapons
- BallisticMissileOld
- MissileSpawnerOldMaster/Slave
- AffectedByTemporal

---

## Development Checklist

### New Unit
- [ ] Add actor in `rules/{faction}-{type}.yaml`
- [ ] Add sprites in `sequences/{type}.yaml`
- [ ] Run `make test`
- [ ] Test in-game

### New Trait
- [ ] Create `Traits/{Name}.cs`
- [ ] Inherit from ConditionalTrait
- [ ] Build: `make`
- [ ] Add to YAML
- [ ] Test: `make test`

### New Warhead
- [ ] Create `Warheads/{Name}.cs`
- [ ] Inherit from Warhead
- [ ] Add to `weapons/{file}.yaml`
- [ ] Build: `make`
- [ ] Test: `make test`

### New Weapon
- [ ] Define in `weapons/{file}.yaml`
- [ ] Add projectile type
- [ ] Chain warheads
- [ ] Reference in unit
- [ ] Test: `make test`

---

## Common Commands

```bash
# Build everything
make

# Validate YAML syntax
make test

# Check code style
make check

# Run the game
./launch-game.sh

# Search for a unit
grep -r "unitname:" mods/rv/rules/

# Search for a trait
grep -r "TraitName:" mods/rv/rules/

# Search for a weapon
grep -r "Weapon: weaponname" mods/rv/rules/

# Find condition usage
grep -r "RequiresCondition:" mods/rv/rules/
```

---

## Understanding the Flow

### How a Unit Attacks

1. **Definition** (`rules/{faction}-vehicles.yaml`):
   ```yaml
   tank:
     Armament@primary:
       Weapon: tankgun
   ```

2. **Trait** (OpenRA.Mods.Common, AttackTurreted):
   - Responds to attack orders
   - Manages targeting and facing

3. **Weapon** (`weapons/bullets.yaml`):
   ```yaml
   tankgun:
     Projectile: BulletAS
       Speed: 600
     Warhead@1: SpreadDamage
   ```

4. **Activity**:
   - Starts projectile motion
   - Handles hit detection

5. **Warhead Impact** (`Warheads/StealResourceWarhead.cs`):
   - Executes custom logic (damage, effects, etc.)
   - Can spawn new projectiles or units

---

## File Size Reference

- Entire C# project: ~2000 lines
- Entire YAML rules: ~51,795 lines
- Total assets: Many MB (sprites, audio)

---

## External References

**OpenRA Documentation**: https://github.com/OpenRA/OpenRA/wiki

**OpenRA Mod SDK**: https://github.com/OpenRA/ModSDK

**Related Mods**:
- RA2 (Base engine compatibility)
- CNC (C&C mechanics)
- AS (Additional support library)

---

## Best Practices When Reading Code

1. **Start with YAML, move to C#**: Understand what you're reading first
2. **Follow inheritance chains**: Use `Inherits:` to understand base traits
3. **Search for conditions**: Use `RequiresCondition:` to find related logic
4. **Check interfaces**: Look at TraitsInterfaces.cs for custom event hooks
5. **Test as you learn**: Use `make test` after changes
6. **Use grep heavily**: The codebase is large, search is your friend

---

Generated for Romanov's Vengeance OpenRA mod
