# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Terraria3D is a 3D remake of Terraria built with **Godot Engine 4.8** and **C# (.NET 10 / C# 13)**. It uses the Godot Voxel module for a fully destructible block world with procedural terrain generation. Currently in early development — not playable.

## Build & Development Commands

```bash
# Restore .NET dependencies
dotnet restore

# Build the C# project
dotnet build

# Format C# code (excludes addons/)
dotnet format Terraria3D.sln --exclude addons

# Format Python scripts (requires ruff)
ruff format

# Run all pre-commit formatters
python scripts/pre-commit.py

# Run spell-check (requires typos-cli)
typos
```

## Coding Conventions

### No `GetNode` — use `[Export]` for node references

**Strict rule: never use `GetNode<T>()` or `GetNode()` to obtain node references.** Always declare an exported property and set it in the editor via the `.tscn` file.

```csharp
// ✗ WRONG — GetNode
private UIManager _uiManager;
public override void _Ready()
{
    _uiManager = GetNode<UIManager>("/root/StartGame/UIManager");
}

// ✓ RIGHT — [Export] property, set in editor
[Export]
public UIManager UiManager { get; set; }
```

**Why:** This keeps node wiring visible and editable in the Godot editor rather than buried in code. It also avoids string-based paths that break silently when the scene tree changes.

**No exceptions.** This includes:
- Child node access (`GetNode("Child")` → `[Export] public Node Child { get; set; }`)
- Sibling/parent access (`GetNode("../Sibling")` → `[Export] public Node Sibling { get; set; }`)
- Absolute paths (`GetNode("/root/Foo/Bar")` → `[Export] public Bar Bar { get; set; }`)

### Never edit `.tscn` files directly — always use Godot AI MCP

**Strict rule: `.tscn` scene files must NEVER be modified by hand, via `sed`, `Write`/`Edit` tools, or any Python script.** Scene files are complex and fragile; direct edits risk corrupting resource references, breaking UIDs, and silently losing data (textures, node hierarchy, signal connections).

All scene modifications — adding/removing nodes, changing script references, setting exported properties, updating `node_paths`, or changing signal connections — must go through the **Godot AI MCP** tools (`mcp__godot-ai__*`):

| Task | Use this MCP tool |
|---|---|
| Add `[Export]` node references | `batch_execute` with `node_set_property` to set NodePath values |
| Change script references | `script_attach` or `batch_execute` |
| Spawn new nodes | `node_create` |
| Move/reparent nodes | `node_manage` (`reparent`, `move`) |
| Wire signals | `signal_manage` (`connect`) |
| Update UI layout | `ui_manage` (`set_anchor_preset`, `build_layout`) |

The `.tscn` format is Godot's internal serialization — Godot itself is the only safe writer. The MCP tools go through the editor's undo system and maintain referential integrity.

**No exceptions.** If the MCP tool doesn't support a needed operation, ask the user — never fall back to direct file edits.

### Testing
The project uses gdUnit4. Tests live in `res://tests/`. Test configuration is in `.runsettings.template`. Tests run in CI via the `MikeSchulze/gdunit4-action` GitHub Action. There is no straightforward CLI command to run tests locally — tests run inside the Godot editor via the gdUnit4 plugin, or via `dotnet test` with the gdUnit4 test adapter.

## High-Level Architecture

### Scene Flow
1. **`loading.tscn`** — splash screen (random background, fades in/out over 3s)
2. **`start_game.tscn`** — main menu with Start/Settings/Exit buttons
3. **`main.tscn`** — the game world, containing:
   - **Main** (Node3D + `Main.cs`) — central controller; loads/creates worlds, manages mouse capture, holds `AtlasData`/`SharedData` for block textures
   - **VoxelTerrain** — the voxel world node, driven by `Generator.cs` (a `VoxelGeneratorScript` using `FastNoiseLite` with frequency 0.0055, 4 fractal octaves). Uses `VoxelMesherBlocky` at scale 0.1
   - **Player** (CharacterBody3D + `Player.cs`) — first-person controller (WASD movement, mouse-look, jumping, health/mana stats)
   - **MainGameUI** (Control + `MainGameUi.cs`) — HUD overlay (hearts, stars, inventory)
   - **WorldEnvironment** — procedural sky, DirectionalLight3D with shadows

### Voxel World Pipeline
- `Generator.cs` — procedural terrain generation (`VoxelGeneratorScript`), fills dirt into a `VoxelBuffer`
- `blocks_library.tres` — defines block types (air, dirt, grass) as cube models with atlas textures
- `WorldFile.cs` — world metadata saved as JSON `.wld` files; chunk data stored in `Chunks/` subdirectory
- `WorldSaver.cs` — `VoxelStreamScript` stub for per-block save/load (not yet implemented)

### Key Data Structures
- **Chunk size:** 16×16×16 (`Constants.cs`)
- **Block size:** 0.5 world units
- **Y height limit:** 8400
- **Render distance:** 9 chunks
- **Inventory:** 5 rows × 10 columns (`Inventory.cs`)

### UI Layer
- **C# files** under `src/ui/` — all UI is now C#: `MainGameUi.cs` (HUD), `InventoryUI.cs` / `ItemGrid.cs` (inventory grid), `StartGame.cs` (main menu), `PlayerChoose.cs` (character selection), `UIManager.cs` (menu navigation), `SettingsPanel.cs` (settings), `Hearts.cs` / `Stars.cs` (health/mana display), `ButtonBase.cs` + subclasses (menu buttons with hover animations), `Loading.cs` (splash screen), and more
- Shared button behavior via `ButtonBase.cs` (scale-up on hover to 1.4x)
- Custom theme: `resources/terraria.tres`

### Localization
English (`en-US/`) and Simplified Chinese (`zh-Hans/`) via JSON files, converted to Godot translation format via `scripts/terraria_translate_to_godot.py`. Translation data sourced from original Terraria.

### Dev Tooling
- **Python scripts** in `scripts/` managed by `uv` (Python 3.12, deps: pillow, rectpack)
- `scripts/generate-atlas.py` — packs tile textures into an atlas, outputs JSON metadata
- `scripts/pre-commit.py` — runs `dotnet format` and `ruff format`

### CI (`.github/workflows/`)
| Workflow | What it checks |
|---|---|
| `ci.yml` | Spell-check with `typos` |
| `dotnet.yml` | `dotnet restore` → `dotnet format --verify-no-changes` → `dotnet build` → gdUnit4 tests |
| `gdscript.yml` | `gdformat -c src` (skipped if no .gd files found)

### Editor Plugins
- **gdUnit4** (`addons/gdUnit4/`) — test framework for GDScript and C#
- **godot_ai** (`addons/godot_ai/`) — MCP server providing AI-assisted editor capabilities (camera, materials, particles, UI authoring, scene management, script editing, etc.)

### Physics & Rendering
- Physics: **Jolt Physics** (3D gravity: 23.826)
- Renderer: **Forward Plus** with occlusion culling enabled
- Input actions: `move_forward` (W), `move_back` (S), `move_left` (A), `move_right` (D), `jump` (Space), `escape`
