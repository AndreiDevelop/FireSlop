# Furniture Models

Delivered 2026-11-24. Scope: Blender models and reusable Unity prefabs for the five expansion furniture concepts. This is the user's requested production follow-up to concept step 9.

## Delivered assets

Fridge, Stove, DiningChair, CoatRack and ShoeCabinet each have an editable `source.blend` and `model-preview.png` under `Documentation/Art/Props/<Name>/`, an FBX under `Assets/Models/Props/`, a connected wrapper prefab under `Assets/Prefabs/Props/Furniture/`, and a physics SO under `Assets/SO/Furniture/`. The [production index](../../Art/ModelProduction/README.md#expansion-furniture) contains dimensions, triangle counts, initial masses and all source/prefab links.

The source files retain named construction parts, shared solid-color materials, metric units and studio cameras. FBX exports use one combined mesh per subject, unit-scale ground-center roots and Unity +Z facing. Existing furniture materials and Medium mesh compression are reused; only sage enamel is a new material. No textures or packages were added. Doors and panels are modeled closed; no opening interaction was added.

Prefabs reuse the existing water-push receiver, physics SO, slide material and navigation components. Chair and coat rack have compound boxes instead of a solid bounding volume. Physics masses are initial tuning, not approved final balance. Each gameplay instance needs its own content-ID override and existing scene restoration references when placed.

## Verification

Environment: Blender 5.2.2 LTS and the running Unity 6000.6.0f1 macOS Editor. Unity CLI performed import, material remapping, prefab authoring, saved-asset inspection and rendering. The sandbox initially hid the live connection; authorized elevated CLI access worked. Blender also required elevated execution after its sandboxed launch crashed. The CLI update check could not reach Unity's CDN; the installed CLI supported every required command. No MCP or UI fallback was used.

- All source parts have closed manifold geometry, nonzero-area faces and unit scale. Source triangle totals match FBX exports. All five FBX files were round-tripped through Blender with matching metric dimensions and ground pivots. [Results](../../Art/ModelProduction/furniture-fbx-validation.json).
- Reloaded all five saved Unity prefab assets. Checked identity roots, imported dimensions within 4 mm of source, ground position within 2 mm, matching total triangle counts, one MeshRenderer per model, persistent shared URP materials, nested FBX connections, enabled primitive colliders, Rigidbody ownership, valid SO references, furniture layer and no missing scripts. [Results](../../Art/ModelProduction/furniture-unity-validation.json).
- Reopened `FurnitureReview.unity`: exactly six connected furniture instances, five new and the existing coffee table, active at ground level. The scene is not in build settings. Its actual [Unity render](../../Art/ModelProduction/furniture-unity-preview.png) was visually inspected for front direction, silhouettes, four stove plates/controls, two fridge doors, chair construction and two cabinet panels.
- One temporary Play Mode session confirmed advancing frames. For each furniture type, instantiated two copies in an isolated local physics scene. Awake applied the configured mass/damping and stationary-carving settings. A physics ray hit its authored collider and dispatched a `WaterImpact` through the existing `IWaterTarget` receiver. The hit instance moved; the second remained unchanged. Test gravity was disabled to isolate the impulse. [Results](../../Art/ModelProduction/furniture-physics-validation.json).
- Fixed two issues found during verification: FBX forward orientation and degenerate bevel faces on thin stove panels. Final source/round-trip, prefab and render checks passed after those repairs. The physics check preceded the final bevel-only repair; component settings and collider geometry were unchanged.

The Editor was returned to Edit Mode and temporary background overrides were restored. Final Console ground-truth status reported zero errors, zero warnings and no compilation failure. The older Pipeline history counter was left untouched. No runtime scripts were changed and no player build was created. Main, Intro and HUD scene files were not saved or changed. Integration into playable rooms, scene-specific save IDs, NavMesh route checks around new placements and physical-device playtesting remain outside this asset-delivery pass.

## Recheck

Use Unity CLI with `--project-path /Users/andrii/Projects/GametekaForge/FireSlop --caller plugin --skill unity-cli`. Run `script run --file AgentScripts/FurnitureVerification.cs --entry FireSlop.Verification.FurnitureVerification.CheckAssets`, then `CheckReview` or `CaptureReview`. `FurniturePhysicsVerification.Run` in the companion AgentScripts file requires Play Mode and cleans up its temporary scene. Authoring scripts were temporary; the saved Blender files, materials, prefabs and review scene are the maintained visual source of truth.
