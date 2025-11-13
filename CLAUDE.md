# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Romanov's Vengeance is a third-party OpenRA mod based on Red Alert 2, built using the OpenRA Mod SDK. The project aims to create a balanced multiplayer experience with improvements from OpenRA and modern Command & Conquer games.

**Key Components:**
- **Mod ID**: `rv` (referenced throughout YAML files)
- **Engine**: OpenRA (fetched from custom fork at https://github.com/MustaphaTR/OpenRA)
- **Primary Language**: C# (.NET SDK) for game logic, YAML for data/configuration
- **Solution**: `OpenRA.Mods.RA2.sln` contains the custom mod code

## Building and Running

### Initial Setup

Before building for the first time, fetch the OpenRA engine:
```bash
./fetch-engine.sh
```

This downloads the engine version specified in `mod.config` (ENGINE_VERSION) and extracts it to `./engine`.

### Building the Mod

```bash
# Build with .NET (default, recommended)
make

# Build with Mono (legacy, requires Mono >= 6.4)
make RUNTIME=mono

# Clean build artifacts
make clean
```

The build compiles both the engine (from `./engine`) and the mod project (`OpenRA.Mods.RA2`).

### Running the Game

```bash
# Launch the game
./launch-game.sh       # Linux/macOS
launch-game.cmd        # Windows

# Launch dedicated server
./launch-dedicated.sh  # Linux/macOS
launch-dedicated.cmd   # Windows
```

### Testing and Validation

```bash
# Check YAML syntax for errors
make test

# Check Lua scripts for syntax errors
make check-scripts

# Run StyleCop checks on C# code
make check

# Set mod version
make version VERSION="custom-version"
```

### Utility Commands

The `utility.sh` / `utility.cmd` scripts provide various development tools:
```bash
# Check YAML validity
./utility.sh --check-yaml

# Check explicit interface violations
./utility.sh --check-explicit-interfaces

# Check conditional trait interface overrides
./utility.sh --check-conditional-trait-interface-overrides
```

## Code Architecture

### C# Project Structure

The `OpenRA.Mods.RA2` project contains custom game logic organized into:

- **Activities/** - Actor activity implementations (e.g., `InfectRV`, `BallisticMissileFlyOld`, `HeliDeployForGrantedCondition`)
- **Traits/** - Actor trait implementations defining unit/structure behaviors
  - **Traits/Conditions/** - Conditional trait modifiers (e.g., `GrantConditionOnOwnerLost`, `HeliGrantConditionOnDeploy`)
  - **Traits/Render/** - Visual rendering traits
  - **Traits/Sound/** - Audio-related traits
- **Warheads/** - Weapon warhead effects (e.g., `SpawnActorOrWeaponWarhead`, `TemporalDamageWarhead`, `StealResourceWarhead`)
- **Widgets/** - UI widget implementations
- **PaletteEffects/** - Color palette manipulation (e.g., `ColorAlphaFlashPaletteEffect`)
- **FileSystem/** - Custom file system handlers
- **UtilityCommands/** - Development utility commands (e.g., `ImportRA2MapCommand`)

**Key Interface File**: `TraitsInterfaces.cs` defines mod-specific interfaces like `INotifyEnteredGarrison`, `INotifyExitedGarrison`, `INotifyGarrisonerEntered`, `INotifyGarrisonerExited`.

### Mod Dependencies

The project references engine modules (located in `./engine` after fetch):
- `OpenRA.Game.csproj` - Core engine
- `OpenRA.Mods.Common.csproj` - Common traits shared across mods
- `OpenRA.Mods.Cnc.csproj` - C&C-specific functionality
- `OpenRA.Mods.AS.csproj` - Additional mod support library

All references use `<Private>False</Private>` to avoid copying engine DLLs.

### YAML Configuration Structure

Game data is defined in YAML files under `mods/rv/`:

- **mod.yaml** - Main mod manifest defining assemblies, file system, rules, sequences, chrome layouts, etc.
- **rules/** - Actor definitions organized by faction and type:
  - `defaults.yaml` - Base trait definitions
  - `{faction}-{type}.yaml` - Faction-specific units (Allied/Soviet/Yuri/Bakuvian)
  - `world.yaml`, `player.yaml`, `palettes.yaml` - Global configurations
  - `cpowers.yaml` - Superweapons/special powers
  - `upgrades.yaml` - Upgrade system definitions
- **sequences/** - Sprite/animation sequences matching the rules structure
- **weapons/** - Weapon definitions (bullets, missiles, explosions, etc.)
- **chrome/** - UI layout definitions
- **fluent/** - Localization strings using Fluent format
- **tilesets/** - Map tileset definitions (temperat, snow, urban, lunar, arrakis, etc.)
- **audio/** - Sound/music/voice definitions

### OpenRA Architecture Concepts

**Traits**: Composable components that define actor behavior. Each actor (unit/structure) is a collection of traits defined in YAML.

**Activities**: State machines that control actor actions (movement, attacking, deploying, etc.). Implemented in C#.

**Warheads**: Effects applied when weapons hit targets. Defined in YAML with C# implementations for custom types.

**Weapons**: Projectiles with targeting rules and warheads. Defined in YAML files under `weapons/`.

**Sequences**: Animation/sprite definitions mapping image files to game states.

## Development Workflow

### Adding New Traits

1. Create C# file in appropriate `OpenRA.Mods.RA2/Traits/` subdirectory
2. Implement trait class inheriting from appropriate base (e.g., `ConditionalTrait`, `PausableConditionalTrait`)
3. Build the project: `make`
4. Define YAML usage in appropriate rules file under `mods/rv/rules/`
5. Test with `make test` to validate YAML

### Adding New Actors

1. Define actor in appropriate rules file (e.g., `mods/rv/rules/soviet-vehicles.yaml`)
2. Define sprite sequences in matching sequences file (e.g., `mods/rv/sequences/vehicles.yaml`)
3. Run `make test` to validate configuration

### Modifying Existing Behavior

1. Locate trait implementation in `OpenRA.Mods.RA2/Traits/` or check engine traits in `./engine/OpenRA.Mods.*/`
2. For engine traits: consider creating mod-specific override if needed
3. For YAML changes: edit files in `mods/rv/rules/` or `mods/rv/weapons/`
4. Always run `make test` after YAML modifications

### Testing Changes

1. Build: `make`
2. Validate YAML: `make test`
3. Launch game: `./launch-game.sh`
4. Test in-game using appropriate maps from `mods/rv/maps/`

## Platform Compatibility

The mod supports:
- **Windows** >= 7 with PowerShell >= 3
- **macOS** >= 10.7
- **Linux** (primary development platform based on .editorconfig)

Use `make.ps1` for PowerShell/Windows, `Makefile` for Unix-like systems.

## Important Configuration Files

- **mod.config** - Core mod settings (MOD_ID, ENGINE_VERSION, packaging options)
- **user.config** - Local overrides (git-ignored, create if needed)
- **.editorconfig** - Code style settings (37KB, comprehensive formatting rules)

## Engine Synchronization

The mod uses a specific engine commit (`ENGINE_VERSION` in mod.config, currently `ac7864a16d00788d859eb3e9188b4ee9104e879f`). When updating:
1. Update `ENGINE_VERSION` in `mod.config`
2. Run `./fetch-engine.sh` to download new engine
3. Rebuild: `make clean && make`
4. Test thoroughly as engine changes may break mod compatibility
