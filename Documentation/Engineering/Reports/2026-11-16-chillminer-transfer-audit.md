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

## Startup and first-person controls

`Intro` loads `Main` in Single mode. `Main` installs the mission models and loads `HUD` additively. The HUD scene context inherits the mission context, so it shares input, tool selection and the entrance target with the player. The scenes own their references and the HUD owns the single EventSystem. Exiting the mission unloads both scene contexts.

The scene-authored transforms are the baseline for the firefighter and drone. A fresh mission takes its initial values from the scene and starts with the hose selected. Reserve `main-house`, `main-house/firefighter` and `main-house/drone` as stable content IDs for the restart contract. Future reset code must restore the authored baselines on the same instances. Reserve the `FireSlop.Settings.` prefix for user preferences.

Unity Editor Play Mode verified startup from both Intro and Main. The firefighter spawned grounded 5.3 m in front of the entrance, facing the door, and the drone was 3.3 m to the side. Reopening the saved scenes confirmed connected firefighter, drone and HUD prefab instances. The HUD contains the joystick for touch, the hose and hammer slots, and the entrance objective with a direction arrow. Its transparent swipe surface routes touch gestures. The joystick and free-look gesture worked simultaneously with two simulated touches, cancellation centered the joystick, and tapping the hammer selected it without moving the player or changing the camera. Keyboard W moved the player to the door and 2 selected the hammer.

Entering Play Mode with the previous no-reload settings caused a native Unity crash. Full scene and domain reload succeeded and is set in EditorSettings. Physical phone and browser input, focus loss and WebGL performance still require verification. The phone model and browser test matrix remain open.
