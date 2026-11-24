# Furniture Placement and Opening Panels

Delivered 2026-11-24. User-authorized follow-up to the [furniture models](2026-11-24-furniture-models.md): place the new furniture in the house, retain clear routes, verify water response and Restart, and open the new appliance/cabinet doors with visible shelves inside.

## Saved content

Main now contains **21 connected furniture instances** with unique authored IDs under each `Building/Floor_N/New Furniture` group:

- Three East Kitchen groups, each with a fridge, stove and dining chair, in the eastern apartments' rear rooms.
- Three East Entryway groups, each with a coat rack and shoe cabinet, along the corridor wall outside the apartment entrance.
- Three West Entryway groups, each with a coat rack and shoe cabinet, inside the western apartment entrance.

The eastern ground-floor sofa moved from the future kitchen into its living room. The western second-floor sofa moved 0.2 m toward the front wall to clear the new entryway cabinet. Other existing furniture transforms and rescue hiding-cabinet references were preserved. New furniture is saved in Edit Mode; nothing generates or duplicates the arrangement at startup.

The Blender fridge, stove and shoe-cabinet sources now contain hollow carcasses, modeled shelves and editable hinge pivots. Their FBX assets retain a static carcass plus independent door meshes. Existing shared materials, model GUIDs and prefab GUIDs are preserved. Nested leaf meshes have saved identity-rotation overrides in their Unity wrappers to compensate for FBX baked-axis child rotations; those overrides are part of the authored prefab.

Each fridge has two independently opening doors, each stove one drop-down oven door, and each shoe cabinet two independently tilting panels: **21 panels** across the house. Existing `Openable`, `SwingOpenView`, `CabinetContextTarget`, transient Zenject `OpenableModel` bindings and the shared Open/Close HUD actions provide the behavior. E and the context button use the same input path. No runtime C# behavior or shared input priority was changed.

Cabinets and appliances use fitted frame, shelf, base-support and leaf Box Colliders. Moving leaves remain physical and continue resolving water hits to the root Rigidbody. Base-support collision retains the authored floor height. The existing furniture physics SO assets and stationary-carving navigation components remain authoritative.

## Verification

Environment: running Unity 6000.6.0f1 macOS Editor through Unity CLI; Blender 5.2.2 LTS for source authoring and geometry checks. No player build was requested or created.

- [Saved scene check](../../Art/ModelProduction/furniture-house-authoring.json): reopened Main, counted 21 new connected prefabs, verified their unique IDs and panel receivers, and found zero penetrations deeper than 2.5 cm against other enabled solid colliders. Contact with the floor is permitted. The uniqueness check covers the new placements; legacy furniture IDs were not migrated.
- [Source and FBX check](../../Art/ModelProduction/furniture-fbx-validation.json): all five sources have closed manifold parts, nonzero-area faces and unit scale. The three opening sources retain editable hinges. Exported geometry totals and hinge names match the sources.
- [Saved prefab check](../../Art/ModelProduction/furniture-unity-validation.json): final dimensions and ground origins match the source, triangle counts match, shared URP materials and nested FBX connections persist, and components/configuration references are valid. Fridge: 660 triangles; stove: 1,532; chair: 352; coat rack: 396; shoe cabinet: 528.
- [Opening and route check](../../Art/ModelProduction/furniture-opening-validation.json): every panel was targeted by an unobstructed physics ray, advertised Open, opened through `InteractionModel.Interact`, advertised Close, and closed through the same shared action. Sibling panel states remained independent. A 0.34 m-radius capsule was swept along 12 kitchen/entryway segments, then again with every new panel open. No new furniture blocked those segments. These checks isolate furniture clearance; intentional fire blockers remain gameplay obstacles.
- The test session's fires were extinguished through `WaterFireTarget.ApplyWater` and fire spread was temporarily stopped so intentional fire blockers could not mask furniture line of sight. The scene was not saved in that state; Restart restored the authored fire setup.
- [Water response](../../Art/ModelProduction/furniture-house-water-validation.json): while panels were open, a real collider ray hit and the existing `IWaterTarget`/`Movable` receiver received a `WaterImpact`. One instance of every furniture type moved more than 1 cm. This is an Editor receiver/physics integration check, not a manual full hose-aiming playthrough.
- [Actual Restart](../../Art/ModelProduction/furniture-restart-validation.json): after moving furniture and opening panels, called the real `MissionController.Restart`. The test set only its in-memory result gate, without firing the victory event or crediting ranking score. Main reloaded with exactly 21 new instances, authored positions within 4 cm after settling, rotations within 1 degree, and all 21 panels closed and unlocked. No additional restore registry or runtime layout creation was added.

Visual inspection covered the [ground-floor layout](../../Art/Environments/BaseHouse/furniture-ground-floor.png) and [open kitchen](../../Art/Environments/BaseHouse/furniture-open-kitchen.png). These are actual Unity inspection renders with upper-floor renderers hidden and temporary inspection lighting/fog overrides. The overrides were restored and not saved. The kitchen image shows the fridge compartments/shelves and the open oven with internal shelves; the entryway panels are visible in the layout render.

## Limits and recheck

Physical-device input, a full manual rescue playthrough and final furniture mass tuning remain unverified. Existing mission doors and hiding cabinets keep their prior behavior. The new furniture is not added to the resident hiding-spot pool. Scene-specific layout and fresh/restored visibility are saved in Main; Restart restores the authored layout by reloading Main.

Final verification returned zero Console errors/warnings and no compilation failure. The saved-scene check again found 21 connected instances with zero overlap candidates. The Editor was left in Edit Mode on Intro with no unsaved scene changes; temporary background overrides were removed.

Unity CLI flags: `--project-path /Users/andrii/Projects/GametekaForge/FireSlop --caller plugin --skill fireslop-interactions`. `FurnitureHouseVerification.CheckSaved` in AgentScripts verifies saved placement in Edit Mode. In a fresh Main Play session, run `FurnitureHousePlayVerification.CheckOpeningAndPaths`, then `CheckPushAndRestart`. The latter restores the tested gameplay scene. `FurnitureVerification.CheckAssets` checks reusable asset references and bounds. Temporary visual authoring scripts were removed after verification; saved Blender files, prefabs and scene placement are the maintained authority.
