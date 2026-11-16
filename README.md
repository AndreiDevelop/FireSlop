# FireSlop

Open this repository root in Unity **6000.6.0f1**. The project uses the Universal 3D (URP) template. Desktop and mobile browsers are the intended WebGL targets.

- [Project instructions](AGENTS.md)
- [Code style and architecture](Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md)
- [Documentation guidelines](Documentation/DOCUMENTATION_GUIDELINES.md)
- [GDD best practices](Documentation/GDD/GDD_BEST_PRACTICES.md)
- [Art direction guidelines](Documentation/Art/ART_DIRECTION_GUIDELINES.md)

`Assets/Scenes/SampleScene.unity` is the starter scene. Intro, Main and the additively loaded HUD are pending implementation. WebGL builds and browser validation are pending.

Open **Tools → Compress Assets** for the texture, model and audio tools. **Show uncompressed assets** scans import settings without changing them. The initial WebGL texture size limit is 2048, and audio optimization initially targets WebGL AAC. Review the selected assets and settings before applying changes.
