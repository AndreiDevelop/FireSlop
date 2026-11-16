# ChillMiner transfer audit

Date: 2026-11-16. Scope: initial source and dependency review for plan item 01.1. No assets or scripts have been ported.

ChillMiner source: `../DigMerge/Dig-Merge`, revision `82d46201542d103442824009b906b58550cc51ef`, clean working tree at inspection. Both projects use Unity 6000.6.0f1, Input System 1.20.0 and uGUI 2.6.0. ChillMiner vendors Zenject under `Assets/Plugins/Zenject` with an MIT license; retain its notice if copied. Verify provenance separately for any imported art, audio or other vendor assets.

| Source under ChillMiner's `Assets/Scripts/` | Recommended use |
| --- | --- |
| `Dig/Move/PlayerController.cs`, `Dig/Move/MobileLookModel.cs`, `Movement/MobileMovementModel.cs` | Adapt movement and look. The controller uses `CharacterController`, Input System, `PlayerInputActions`, and Zenject bindings for camera, interaction and mobile state. Separate locomotion from the source interaction stack. |
| `Interaction/Controllers/InteractionInputController.cs`, `Interaction/Views/InteractionActionButton.cs` | Reuse input and cancellation patterns for the two tools and contextual interaction. |
| `Toolbelt/ToolbeltModel.cs`, `Toolbelt/ToolbeltSlotView.cs` | Adapt slot selection and display. The source model depends on Dig and Equipment; use two fixed FireSlop slots without that dependency. |
| `Tutorial/Models/QuestTrackerModel.cs` | Reuse the HUD presentation pattern. The source model references inventory, expedition, museum, progression and other game modules; use one FireSlop rescue objective. |
| `Dig/Terrain/VoxelWorld.cs`, `Dig/Terrain/VoxelChunk.cs`, `Dig/Terrain/VoxelTerrainConfigSO.cs` | Compare with prepared door and wall sections in step 07. The existing world references Dig and Inventory; avoid bringing in the entire terrain system. |

FireSlop already has an authored `Main` with a `Player` instance based on `Resident.prefab` and a `Drone` instance based on `RescueDrone.prefab`. The enabled startup scene is still `SampleScene`; Intro, HUD, and Zenject composition remain to be set up. The placement requested for Main is the marked path in front of the entrance for the player and an adjacent ground position for the drone, with the approach left clear. Verify placement and prefab connections in the live Editor during scene authoring.

This review checked source files, the package manifests, the saved Main scene and the build scene list. It did not verify compilation, startup, placement in the live Editor or desktop/touch behavior. Exact files and asset references to copy should be confirmed when each feature is adapted.
