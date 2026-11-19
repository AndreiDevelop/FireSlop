# FireSlop

Open this repository root in Unity **6000.6.0f1**. The project uses the Universal 3D (URP) template. Desktop and mobile browsers are the intended WebGL targets.

- [Project instructions](AGENTS.md)
- [Code style and architecture](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md)
- [Documentation guidelines](Documentation/DOCUMENTATION_GUIDELINES.md)
- [GDD best practices](Documentation/GDD/GDD_BEST_PRACTICES.md)
- [Art direction guidelines](Documentation/Art/ART_DIRECTION_GUIDELINES.md)

## Run in the Editor

Open `Assets/Scenes/Intro.unity` and enter Play Mode. Intro loads Main, which loads HUD additively. Main owns gameplay and HUD owns the runtime interface.

Move with WASD or the arrow keys, look with the mouse, use the left mouse button for the selected tool, and press E for the displayed context action. Tool shortcuts are shown in the HUD. Touch controls are available for mobile input.

Try extinguishing a fire, breaking an obstacle with the hammer, opening a cabinet and carrying the resident to the rescue zone. The project is an in-progress rescue-game prototype targeting desktop and mobile WebGL. See the [implementation plan](FireSlop/Documentation/GDD/ImplementationPlan.md) for completed work and remaining acceptance checks.

## Code entry points

- [Startup](Assets/Scripts/Startup/) contains the Intro and additive HUD loading flow.
- [Firefighter movement](Assets/Scripts/Firefighter/Movement/) handles movement and traversal.
- [Interactions](Assets/Scripts/Interaction/) contains context actions, water, destruction and rescue.
- [Mission HUD](Assets/Scripts/MissionHud/) contains tool selection and gameplay UI.

The project uses Model / Controller / View responsibilities, Zenject dependency injection and ScriptableObject configuration. The [architecture guide](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md) defines the intended conventions.

## Asset tools

Open **Tools → Compress Assets** for the texture, model and audio tools. **Show uncompressed assets** scans import settings without changing them. The initial WebGL texture size limit is 2048, and audio optimization initially targets WebGL AAC. Review the selected assets and settings before applying changes.
