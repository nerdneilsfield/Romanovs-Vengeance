================================================================================
CODEBASE DOCUMENTATION - READ ME FIRST
================================================================================

Two comprehensive guides have been created to help you navigate and understand
the Romanov's Vengeance OpenRA mod codebase:

1. CODEBASE_GUIDE.md
   - Comprehensive reference (12 sections, 17KB)
   - For: In-depth understanding and complete reference
   - Contains: Architecture, file organization, patterns, workflows, checklists

2. CODEBASE_STRUCTURE.md  
   - Quick reference (practical guide, 7.3KB)
   - For: Rapid navigation and quick lookup
   - Contains: Quick nav, directory map, examples, commands

START HERE:
   For first-time readers: Read CODEBASE_GUIDE.md section 1-4
   For quick answers: Use CODEBASE_STRUCTURE.md

KEY ENTRY POINTS TO START READING:

Simple Examples (easiest to understand):
   - OpenRA.Mods.RA2/Traits/WithAcceptDeliveredCashSound.cs (30 lines)
   - OpenRA.Mods.RA2/TraitsInterfaces.cs (27 lines)
   - OpenRA.Mods.RA2/Warheads/StealResourceWarhead.cs (52 lines)

Architecture Overview:
   - mods/rv/mod.yaml (main configuration)
   - mods/rv/rules/defaults.yaml (base templates)

Complex Example (understand after basics):
   - mods/rv/rules/soviet-vehicles.yaml (harvester unit)

QUICK STATS:
   - C# Code: 23 files, ~2000 lines
   - YAML Config: 45+ files, ~51,795 lines
   - Custom Traits: 10
   - Custom Activities: 4
   - Custom Warheads: 5
   - Factions: 4 (Allied, Soviet, Yuri, Bakuvian)

KEY DIRECTORIES:

C# Code:
   OpenRA.Mods.RA2/              - Main custom code
     ├─ Traits/                  - Gameplay mechanics (10 files)
     ├─ Activities/              - State machines (4 files)
     ├─ Warheads/                - Impact effects (5 files)
     └─ [Render, Sound, UI, etc]

YAML Configuration:
   mods/rv/                      - Game configuration
     ├─ rules/                   - Actor definitions (45 files)
     ├─ sequences/               - Animations (23 files)
     ├─ weapons/                 - Weapon definitions (14 files)
     └─ [audio, chrome, tilesets, bits, etc]

ENGINE:
   engine/                       - OpenRA engine fork (auto-fetched)

NEXT STEPS:

1. Open: CODEBASE_GUIDE.md
2. Read: Section 1 (Project Overview)
3. Read: Section 2 (Architecture Overview)
4. Choose your learning path from Section 4
5. Use Section 3 as directory reference

For quick reference:
   Open: CODEBASE_STRUCTURE.md
   Sections: "Quick Navigation" or "Directory Map"

BUILD & RUN:

   Build:   make
   Test:    make test
   Run:     ./launch-game.sh

CUSTOM FEATURES TO UNDERSTAND:

   - Infection System (virus units)
   - Mirage Units (decoy mechanics)
   - Helicopter Deployment
   - Temporal Damage
   - Resource Theft
   - Garrison System

All files documented with complete file paths and complexity levels.

Generated: November 13, 2025
