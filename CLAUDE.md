# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Light_and_Shadow** is a tModLoader mod for Terraria about the story of Light and Shadow. It adds crystal items, weapons, a minion, and world generation features including tree houses and living tree stele shrines.

**Important:** This repository also contains `Content/ImproveGame/` - a separate, unrelated tModLoader mod project (ImproveGame - Quality of Terraria) with its own `.csproj`. Do not confuse it with the Light_and_Shadow mod.

## Build Instructions

This is a standard tModLoader mod project using the MSBuildSdk:

1. Build via Visual Studio or `dotnet build`
2. The mod uses tModLoader's build system via `tModLoader.targets` imported in the .csproj
3. After building, the mod is enabled via tModLoader's mod browser/loader

## Architecture

### Light_and_Shadow Mod Structure

- **`Light_and_Shadow.cs`** - Main mod entry point (currently empty skeleton)
- **`Common/Systems/`** - World generation systems:
  - `TreeHouseGenerator.cs` - Loads `.qotstruct` structure files and places tree houses during Final Cleanup worldgen pass
  - `LivingTreeSteleGenerator.cs` - Grows living trees with stele shrines during world generation
- **`Common/Players/`** - ModPlayer classes:
  - `SummonAddBuff.cs` - Adds extra summon slots based on game stage (pre-hardmode +0, post-hardmode +1, post-plantera +3)
- **`Content/Items/`** - Items organized by subfolder (Blocks, Stuffs, Weapons)
- **`Content/Tiles/`** - Custom tiles (Crystal tiles, MysteriousStoneStele)
- **`Content/Projectiles/Minions/`** - Summon projectiles (RainbowSummon)
- **`Structure/`** - Contains `.qotstruct` files (binary structure data for world generation)
- **`Localization/`** - `.hjson` localization files (en-US currently)

### Key Patterns

- **ModSystem** for world generation via `ModifyWorldGenTasks` hook
- **ModPlayer** for player modifications via inheritance
- **Structure files** (`.qotstruct`) are Terraria's native structure format, loaded via `TagIO.FromFile`
- **ModContent.TryFind** for resolving mod tile/wall names to type indices

### Structure File Format

The `.qotstruct` files contain:
- `Width`, `Height`, `OriginX`, `OriginY` - structure metadata
- `EntriesName`, `EntriesType` - mappings from mod tile/wall names to type indices
- `StructureData` - tile data array with TileIndex, WallIndex, TileFrameX, TileFrameY

### ImproveGame Subproject

`Content/ImproveGame/` is a separate tModLoader mod with its own project file and architecture documented in `Content/ImproveGame/CLAUDE.md`. It has extensive UI systems, configurations, and features unrelated to Light_and_Shadow.
