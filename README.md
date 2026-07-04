# SandBoxGame — Project Documentation

Godot 4.5.1 voxel-based sandbox game with C# (.NET 8.0).
Procedural terrain, construct-based architecture, distance-based simulation LOD,
and modular block/ability system.

---

## Project Structure

```
SandBoxGame/
├── SandBoxGame.sln              # Solution file
├── SandBoxGame.csproj           # Godot.NET.Sdk/4.5.1, net8.0, C# project
├── project.godot                # Godot project config
├── icon.svg / icon.svg.import   # Project icon
│
├── src/                         # C# source code
├── scenes/                      # Godot scene files (.tscn)
├── resources/                   # Godot resources (.tres, .gdshader)
├── doc/                         # Architecture diagrams
├── tutorial/                    # Tutorial shader/scene
└── archive/                     # Archived/legacy assets
```

---

## Entry Point & Core Game Loop

### `src/GameController.cs`
**Main game loop controller.** Runs each physics frame, tracks camera position, and
notifies `ConstructWorld` when the camera moves (triggers loading/unloading).
Also handles scene initialization — finds child `ConstructNode`s and creates
`Construct` instances from them.

### `src/GameSettings.cs`
**Global singleton settings.** Holds seed, module size (default 32), material,
and simulation mode distance thresholds (ACTIVE / APPROXIMATED / FROZEN).
Distances are ordered by value and used to determine simulation fidelity per
construct based on distance from camera.

### `src/CameraTemp.cs`
**Temporary flycam controller.** WASD + sprint movement, mouse rotation.
Moves relative to camera orientation. Exists for development/debugging.

---

## World System

### `src/world/ConstructWorld.cs`
**Root of the world scene.** Implements `IWorldQuery`. Maintains an
`ExpandingOctTree<Construct>` for spatial queries and a `HashSet<Construct>`
for iteration. Dispatches `UpdateLoading()` to all constructs when camera
moves. Constructs marked as static are inserted as "global" (always returned
in queries) rather than spatially tracked.

### `src/world/IWorldCollisionQuery.cs`
**Interface for world queries.** Used by constructs to check collisions.
Two methods: `HasBlockAt(worldPos)` and `GetConstructsInArea(min, max)`.

### `src/world/BlockStore.cs`
**Global singleton block registry.** Holds an array of `BlockDefault` resources
(exported in the Godot editor). Assigns sequential IDs starting at 1.
Lookup by ID or `Block` value.

---

## Block Types

### `src/world/blocks/Block.cs` (in `Module/Block.cs`)
**Value-type block identifier.** A `readonly record struct` with `Id`, `Direction`,
and `Orientation`. `IsEmpty` (Id == 0) represents air.

### `src/world/blocks/BlockDefault.cs`
**Base block definition resource.** Fields: `Name`, `Health`, `Weight`,
`PassiveAbilities[]`, `ActiveAbilities[]`. ID is set once by `BlockStore` and
immutable thereafter.

### `src/world/blocks/ModelBlockDefault.cs` extends BlockDefault
**Mesh-based block.** Has a Godot `Mesh` and a `BlockFaceResource` for LOD faces.
Used for complex/non-cubic blocks rendered via `MultiMeshInstance3D`
(managed by `ConstructModelBlockController`).

### `src/world/blocks/VoxelBlockDefault.cs` extends BlockDefault
**Greedy-mesh-optimized cubic block.** Has per-face `BlockFaceResource` (with
fallback `DefaultFace`). Surfaces are merged into large quads by
`ModuleMeshGenerator`. Managed via `ConstructVisualsController` as `ArrayMesh`.

### `src/world/blocks/BlockFaceResource.cs`
**Texture atlas position and orientation for a block face.**
`TextureAtlasPos` (Vector2I) + `FaceOrientation` (NORTH/EAST/SOUTH/WEST).

### `src/world/blocks/BlockState.cs`
**Runtime block state modifiers.** Tracks `HealthChange`, `WeightChange`,
added/removed abilities, and ability cooldowns. Not yet wired into the
simulation loop.

---

## Abilities

### `src/world/abilities/active_abilities/ActiveAbility.cs`
**Base class for active abilities.** Trigger types: `RandomTick`, `OnUpdated`,
`PlayerForward/Backward/Left/Right`. Has `RefireCooldown`. Base `TriggerAbility()`
throws (must be overridden).

### `src/world/abilities/active_abilities/MoveAbility.cs` extends ActiveAbility
**Placeholder movement ability.** Currently TODO stub.

### `src/world/abilities/passive_abilities/PassiveAbility.cs`
**Base class for passive abilities.** Currently empty — a marker resource.

---

## Construct System

The core architecture: a "Construct" is a collection of blocks organized into
modules (chunks of `moduleSize`³), with simulation fidelity that scales with
distance from the camera.

### `src/world/construct/Construct.cs`
**The main construct class.** A `Node3D` implementing `IOctTreeObject`.
Created via factory `GetInitializedConstruct()`. Holds `ConstructCore` (data),
`ConstructBlockService` (block operations), and an `IConstructController`
(either `GlobalConstructController` for static terrain or
`SimulationStateController` for dynamic constructs). Routes `_PhysicsProcess`
to the controller's `Update()`.

### `src/world/construct/ConstructCore.cs`
**Data + service container.** Holds `ConstructData` and `ConstructBlockService`.

### `src/world/construct/ConstructNode.cs`
**Godot editor placement node.** Exports `ConstructCreationSettings`.
Factory: `CreateConstruct(IWorldQuery)` → calls `Construct.GetInitializedConstruct()`.

### `src/world/construct/ConstructCreationSettings.cs`
**Construct configuration resource.** Exports: `ConstructGeneratorSettings`
(generator type), `MoveSodSettings`/`RotSodSettings` (Second Order Dynamics
parameters for smooth movement), `IsGlobal` (static vs dynamic).

---

## Construct Data Layer

### `src/world/construct/ConstructData/ConstructData.cs`
**Aggregate data object.** Holds `PhysicsData`, `GridTransform`, `Modules`, `Bounds`.

### `src/world/construct/ConstructData/ConstructPhysicsData.cs`
**Physics state.** `BlockMass`, `Velocity` (Vector3), `PhysicsPosition` (float
— continuous position for physics simulation), `IsStatic`.

### `src/world/construct/ConstructData/ConstructGridTransformData.cs`
**Grid-position state with rotation.** `WorldPos` (grid-aligned int position),
`FacingDirection` (cardinal direction, UP/DOWN excluded), derived `YRotation`
(in degrees). Fires `Changed` event on mutation via property setters.

### `src/world/construct/ConstructData/ConstructModulesData.cs`
**Module collection.** Dictionary `ModuleLocation → Module`. Fires events:
`OnModuleAdded`, `OnModuleRemoved`, `OnModuleChanged` (block-level changes).
Tracks `FullyLoaded` flag for one-shot generation.

### `src/world/construct/ConstructData/ConstructBoundsData.cs`
**AABB bounds derived from modules.** Tracks `MinPos`/`MaxPos` (ConstructGridPos).
Subscribes to `ModulesData.OnModuleChanged` for auto-update. Fires `Changed` event.
Has `IsOnBounds()` for efficient boundary checks used in collision.

---

## Module System

Modules are chunks of `moduleSize`³ (default 32³ = 32,768 possible blocks).

### `src/world/construct/Module/Module.cs`
**A single chunk of blocks.** Flat array (`Block[moduleSize³]`), `BlockCount`,
`SurfaceCacheController` (render + collision surfaces). `SetBlock()` —
single-block placement with surface cache update. `SetBlocks()` — batch
operation, fires `OnModuleChanged` with all changes.

### `src/world/construct/Module/BlockChange.cs`
**Describes a block mutation.** `BlockChangeAction` enum (PLACE/REMOVE),
`ModuleGridPos`, and `Block` value.

### `src/world/construct/Module/ModuleMeshGenerator.cs`
**Greedy mesh generation from surface cache.** Finds rectangular regions of
same-material exposed faces, merges them into large quads, builds `ArrayMesh`
from vertex/index/UV/color arrays. Uses texture atlas — encodes atlas position
in vertex colors (R=atlasX, G=atlasY). Handles block rotation and orientation
for correct face UV mapping.

---

## Surface Cache

### `src/world/construct/surfaceCache/SurfaceCache.cs`
**Generic exposed-surface cache.** Per-direction dictionaries mapping
`ModuleGridPos → BlockSurfaceInfo` (block + block default). Used for both
render (VoxelBlockDefault) and collision (BlockDefault) caches.

### `src/world/construct/surfaceCache/SurfaceCacheController.cs`
**Manages dual caches per module.** `RenderCache` (voxel faces only) and
`CollisionCache` (all solid faces). Three rebuild strategies:
- `RebuildModule()` — full rebuild (sparse or full-module optimization)
- `AddBlock()` — incremental update (handles neighbor occlusion)
- `RemoveBlock()` — incremental update (exposes previously hidden faces)

---

## Controllers

### `src/world/construct/Controllers/ConstructPhysicsController.cs`
**Gravity and velocity integration.** Each frame: applies gravity, moves
`PhysicsData.PhysicsPosition` continuously. When drift from
`GridTransform.WorldPos` exceeds 1 unit, calls `MotionController.TryTakeStep()`
to move the grid position. If step fails (collision), zeros velocity and snaps
physics position back. Maintains `BlockMass` for weight-based physics.

### `src/world/construct/Controllers/ConstructMotionController.cs`
**Grid-position movement with collision.** `TryTakeStep(Direction)` checks all
nearby constructs (via `IWorldQuery`) for block-to-block collision using
surface cache data. `RotateTo()` — rotates the construct around a pivot point.
`TryMoveTo()` / `TryMoveBy()` for arbitrary movement.

### `src/world/construct/Controllers/ConstructVisualMotionController.cs`
**Smooth visual interpolation.** Uses two `SecondOrderDynamics` instances
(one for position, one for Y-rotation) to smoothly interpolate the Godot
`Node3D.Position` and `Rotation` toward the grid-state values. This is what
makes constructs visually glide instead of snapping.

### `src/world/construct/Controllers/ConstructVisualsController.cs`
**Module mesh display.** `Node3D` child of construct. Maintains a pool of
`MeshInstance3D` nodes. `AddModule(loc, mesh)` / `RemoveModule(loc)` manages
the pool. Subscribes to `OnModuleAdded`/`OnModuleRemoved` events from
`ConstructModulesData` (removal is event-driven, addition still uses direct
calls via `ModuleIntegrationHelper` because mesh generation is async).

### `src/world/construct/Controllers/ConstructModelBlockController.cs`
**Model-block rendering via MultiMesh.** For `ModelBlockDefault` blocks
(non-voxel, mesh-based). Maintains one `MultiMeshInstance3D` per distinct
mesh type. Subscribes to all three `ConstructModulesData` events:
- `OnModuleChanged` → updates affected blocks
- `OnModuleAdded` → iterates all blocks in new module, adds non-empty ones
- `OnModuleRemoved` → removes all blocks belonging to that module

---

## Services

### `src/world/construct/Services/ConstructBlockService.cs`
**Block CRUD facade over data layer.** Wraps `ConstructModulesData.SetBlock()` +
`ConstructBoundsData` updates. Handles world-to-construct coordinate conversion.
Used by `Construct` to expose public block API.

---

## Simulation States

Distance-based LOD: constructs near the camera are ACTIVE, further ones are
APPROXIMATED, distant ones are FROZEN.

### `src/world/construct/SimulationState/SimulationStateController/SimulationMode.cs`
**Enum:** `ACTIVE`, `APPROXIMATED`, `FROZEN`.

### `src/world/construct/SimulationState/IConstructController.cs`
**Interface:** `UpdateLoading(WorldGridPos)` and `Update(double delta)`.

### `src/world/construct/SimulationState/SimulationStateController/SimulationStateController.cs`
**State machine for dynamic constructs.** Chooses state based on distance from
camera load position. Transitions via `Exit()` on old state → create new state
→ `Enter()`.

### `src/world/construct/SimulationState/SimulationStateController/SimulationState.cs`
**Abstract base for simulation states.** Holds `ConstructCore`, virtual
`Enter()`/`Exit()`/`Update()`.

### `src/world/construct/SimulationState/SimulationStateController/ActiveState.cs`
**Full simulation.** Creates all controllers (physics, motion, visual motion,
visuals, model blocks, module builder). Runs physics + visual interpolation
each frame. Reports position changes to trigger loading.

### `src/world/construct/SimulationState/SimulationStateController/ApproximatedState.cs`
**Reduced simulation.** Currently empty — placeholder for future LOD behavior.

### `src/world/construct/SimulationState/SimulationStateController/FrozenState.cs`
**No simulation.** Currently empty — construct is completely frozen.

### `src/world/construct/SimulationState/GlobalConstructController.cs`
**Controller for static/global constructs.** No physics, no motion. Only
handles module generation around camera position (`BuildAround`) and
block queries. Creates its own `ConstructVisualsController` and
`ConstructModuleBuilder`.

---

## Module Builder & Integration

### `src/world/construct/ModuleBuilder/ConstructModuleBuilder.cs`
**Async module generation orchestrator.** Semaphore-limited (max 7 concurrent).
`GenerateModulesAround()` — distance-based streaming load/unload calculation
+ task creation. `GenerateAllModules()` — one-shot generation for finite
constructs. `GenerateModuleMesh()` — mesh-only regeneration. Uses
`Task.Run()` for threaded module generation + mesh building.

### `src/world/construct/ModuleBuilder/ModuleBuildSet.cs`
**Simple DTO.** `ToLoad` (List<ModuleLocation>) and `ToUnload` (List<ModuleLocation>).

### `src/world/construct/ModuleBuilder/ModuleMeshGenerateContext.cs`
**Context for mesh generation.** Contains `Module` and `ModuleLocation`.

### `src/world/construct/ConstructBuilder/ModuleIntegrationHelper.cs`
**Shared integration logic.** `IntegrateGeneratedModules()` — adds modules to
data, updates bounds, adds meshes to visuals controller. `UnloadModules()` —
removes from data, removes visuals (now event-driven), rebuilds bounds if needed.

---

## Construct Generators

### `src/world/construct/ConstructGenerator/ConstructGenerator.cs`
**Abstract base.** `GenerateModules(location, prevLoaded?)` → module block data.
`IsModuleNeeded(location)` → streaming load decision. `GetAllRequiredModules()`
→ for finite one-shot loading.

### `src/world/construct/ConstructGenerator/ConstructGeneratorSettings.cs`
**Abstract resource.** Factory: `CreateConstructGenerator(seed)` → `ConstructGenerator`.

### `src/world/construct/ConstructGenerator/ModuleGenerationResponse.cs`
**Block-level generation response.** `GeneratedAllModules` flag + dictionary of
`ModuleLocation → Module`.

### `src/world/construct/ConstructGenerator/StreamingConstructGenerator/BiomeWorldGenerator/BiomeWorldGenerator.cs`
**Infinite terrain generator.** Uses simplex noise + biomes for ground height.
Populates modules column-by-column. Caches ground heights (LRU, 100k entries).
Tracks max module Y per column for `IsModuleNeeded()` optimization.

### `src/world/construct/ConstructGenerator/StreamingConstructGenerator/BiomeWorldGenerator/BiomeWorldGeneratorSettings.cs`
**Editor resource for biome world.** Exports biome list, creates `BiomeWorldGenerator`.

### `src/world/construct/ConstructGenerator/StreamingConstructGenerator/BiomeWorldGenerator/Biomes/Biome.cs`
**Biome definition.** Stack of `NoiseLayer`s for height calculation.
`GetBlock()` — returns block at a world position (can be overridden for
biome-specific logic). `GetGroundHeight()` — sums noise layers.

### `src/world/construct/ConstructGenerator/StreamingConstructGenerator/BiomeWorldGenerator/Biomes/NoiseLayer.cs`
**Single noise octave.** `NoiseScale`, `NoiseHeight`, `HeightPow`, `NoiseHeightOffset`,
noise type. `GetNoiseHeight2D()` evaluates simplex noise.

### `src/world/construct/ConstructGenerator/StreamingConstructGenerator/BiomeWorldGenerator/Structure.cs`
**Structure resource.** Array of `Vector4I` blocks (x, y, z, blockId).
For placing structures in terrain.

### `src/world/construct/ConstructGenerator/StreamingConstructGenerator/BiomeWorldGenerator/StructurePlacement.cs`
**Structure placement rules.** `GridSize`, `Grounded` flag. `GetClosest()` stub
(currently returns `Vector3I.Down`).

### `src/world/construct/ConstructGenerator/FiniteConstructGenerator/PresetConstructGenerator/PresetConstructGenerator.cs`
**Prefab/building generator.** Takes a dictionary of `WorldPos → Block`,
computes required module set. `GenerateModules()` places blocks into appropriate
modules. `GetAllRequiredModules()` returns fixed set (one-shot loading).

### `src/world/construct/ConstructGenerator/FiniteConstructGenerator/PresetConstructGenerator/PresetConstructGeneratorSettings.cs`
**Editor resource for preset constructs.** `Dictionary<Vector3I, GodotBlock>` +
`Offset`. Creates `PresetConstructGenerator`.

### `src/world/construct/ConstructGenerator/FiniteConstructGenerator/PresetConstructGenerator/GodotBlock.cs`
**Editor-friendly block definition.** `BlockId`, `FaceDir`, `Orientation` — used
in exported dictionaries for preset constructs.

---

## Utility Types

### `src/utils/GridPosition.cs`
**Four coordinate system types:**

| Type | Represents |
|---|---|
| `WorldGridPos` | Absolute world grid position |
| `ConstructGridPos` | Position relative to a construct's origin (includes rotation) |
| `ModuleLocation` | Which module/chunk within a construct |
| `ModuleGridPos` | Position within a single module |

All are `readonly record struct` with `Vector3I` backing. Conversion methods
handle rotation transforms (world ↔ construct) and modular arithmetic
(construct ↔ module). Implicit conversions to/from `Vector3I`.

### `src/utils/Direction.cs`
**Cardinal directions + helpers.** `Direction` enum: RIGHT, LEFT, UP, DOWN,
BACKWARD, FORWARD. `DirectionTools` static class: `RotateLeft/Right`,
`Invert`, `GetWorldDirVec` (→ Vector3), `GetClosestDirection` (Vector3 → Direction),
`GetVecFromForward` (relative to arbitrary forward vector).

### `src/utils/Orientation.cs`
**Face rotation enum.** NORTH (0°), EAST (90°), SOUTH (180°), WEST (270°).
Used by `BlockFaceResource` and mesh generator for UV rotation.

### `src/utils/Vector3IBounds.cs`
**Integer 3D bounding box.** Plane-count-based tracking for efficient
`AddPoint`/`RemovePoint` with boundary shrink. `O(1)` add, bounded `O(size)`
shrink. Used by `Module` for tracking occupied volume.

### `src/utils/ExpandingOctTree.cs`
**Generic expanding octree.** `ExpandingOctTree<T> where T : IOctTreeObject`.
Auto-expands root when objects don't fit. Supports spatial insertion,
global objects (always returned in queries), box queries, removal, and
auto-update via `BoundsChanged` event subscription.

### `src/utils/FindChildExtension.cs`
**Godot node extension methods.** `FindChildOfType<T>()` — first match.
`FindChildrenOfType<T>()` — all matches. Direct child only (not recursive).

### `src/utils/TaskExtensions.cs`
**`FireAndForget()` extension.** Attaches fault-only continuation to a Task.
Used for fire-and-forget async operations throughout the codebase.

### `src/utils/SecondOrderDynamics/SecondOrderDynamics.cs`
**Generic Second Order Dynamics filter.** Smooth interpolation with configurable
frequency (f), damping (z), and initial response (r). Parameterized by
`ISecondOrderMath<T>` for float and Vector3 variants. Used for smooth
construct movement and rotation.

### `src/utils/SecondOrderDynamics/SecondOrderDynamicsSettings.cs`
**Godot resource for SOD parameters.** `f`, `z`, `r` — exported to editor.
Factory methods `GetInstance(Vector3)` and `GetInstance(float)`.

### `src/utils/SecondOrderDynamics/ISecondOrderMath.cs`
**Math operations interface.** `Zero`, `Add`, `Sub`, `Mul(float)`, `Div(float)`.

### `src/utils/SecondOrderDynamics/FloatSodMath.cs`
**Float arithmetic implementation.**

### `src/utils/SecondOrderDynamics/Vector3SodMath.cs`
**Vector3 arithmetic implementation.**

### `src/utils/TimeTracking/TimeTracker.cs`
**Performance profiling singleton.** Thread-safe named timers with
`Increment` (total) and `Average` tracking modes. Used in module generation
and movement collision paths. Auto-prints summary each frame.

---

## Scenes

### `scenes/main.tscn`
**Main game scene.** Contains `GameController`, `ConstructWorld`, camera,
lighting, and autoload references.

### `scenes/autoloads/block_store.tscn`
**Block store autoload.** Global singleton, loaded on startup.

### `scenes/autoloads/game_settings.tscn`
**Game settings autoload.** Global singleton with exported parameters.

### `scenes/node_3d.tscn`
**Template/debug scene.**

---

## Resources

### `resources/blocks/`
Block definitions: `basicBlock.tres`, `sphere.tres`, `Arrow.tres`, `blockStore.tres`.

### `resources/movement/`
SOD parameter presets: `MoveMooove.tres`, `MoveMooove2.tres`.

### `resources/textures/`
`world_material.gdshader` + `world_material.tres` — shader/material for module meshes.
Uses vertex colors for texture atlas lookup.

### `resources/world/Constructs/thingy/`
Preset construct: `thingy.tres` (settings) + `thingy_settings.tres` (generator).

### `resources/world/Constructs/world/`
World construct: `World.tres` (settings) + `world_settings.tres` (BiomeWorldGenerator).

### `resources/world/biomes/`
`biomeTest.tres` — test biome definition.

---

## Documentation & Archive

### `doc/`
Architecture diagrams: `Architekturübersicht_game.png` (rendered),
`Architekturübersicht_game.drawio` (source).

### `tutorial/`
`tutorial_scene.tscn` + `tutorial_shader.gdshader` — tutorial/example content.

### `archive/`
`Unbenannt.PNG` — legacy asset.

---

## Key Architectural Notes

### Coordinate Hierarchy
```
WorldGridPos ←→ ConstructGridPos ←→ ModuleLocation + ModuleGridPos
```
Rotation transforms apply at the World↔Construct boundary. Module positions
are always axis-aligned.

### Simulation Flow
1. `GameController._PhysicsProcess()` → camera moved?
2. `ConstructWorld.CameraMoved()` → iterate constructs → `UpdateLoading(cameraPos)`
3. `SimulationStateController.UpdateLoading()` → compute distance → select mode
4. State `Update(delta)` runs physics/visuals each frame
5. Global constructs: `UpdateLoading` triggers `BuildAround()` → async generation

### Generation Pipeline
1. `ConstructModuleBuilder` calculates load/unload sets
2. Thread-pool tasks run `generator.GenerateModules(position)`
3. `SurfaceCacheController.RebuildModule()` computes exposed surfaces
4. `ModuleMeshGenerator.BuildModuleMesh()` creates ArrayMesh from surfaces
5. `ModuleIntegrationHelper.IntegrateGeneratedModules()` adds to data + visuals

### Data Flow (after recent fixes)
```
ConstructBlockService.SetBlock()
  → ConstructModulesData.SetBlock()        [writes data]
    → OnModuleChanged fires
      → ConstructBoundsData recalculates
      → ConstructModelBlockController updates MultiMeshes
      → (ConstructVisualsController does NOT subscribe — handles via AddModule directly)

ModuleIntegrationHelper.UnloadModules()
  → ConstructModulesData.Remove()
    → OnModuleRemoved fires
      → ConstructVisualsController.RemoveModule() cleans up mesh pool
      → ConstructModelBlockController removes all blocks in module
```
