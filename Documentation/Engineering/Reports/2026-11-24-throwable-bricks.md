# Throwable bricks

[Expansion feature 7](../../../FireSlop/Documentation/GDD/Design/SinglePlayerExpansion.md) was verified in Unity 6000.6.0f1 on 2026-11-24.

## Inventory and throwing

The two authored courtyard bricks start in the world. Context pickup stores the same object, equips tool slot four and updates its count. Subsequent pickups add to that stack. The player can switch tools and return using the 4 key or the fourth HUD button. A successful throw spends one unit. After the last throw, the slot disappears and the default hose is selected. A blocked launch preserves the stack.

The equipped brick is a dedicated visual on the canister’s hold point. Stored world bodies are inactive. A throw reactivates one body at the shared launch pose. A window hit uses the existing window impact receiver. The player can recover the brick after impact, and falling below the configured recovery depth returns it to its authored placement.

The belt asset defines the tool, icon, order, shortcut and initial quantity. The standard tool selection model, equipped model view and HUD button handle selection, rendering and quantities. BrickThrowController keeps the stored world instances and handles launch clearance. Both throwable tools receive the same primary input event and use ThrowPoseView, ThrowConfigSO, BallisticTrajectory and the authored ThrowArc prefab. Each tool handles its own release effect. SignalThrowController validates drone landing and dispatches the drone.

## Verification

The Unity CLI checks in AgentScripts/ThrowableBrickVerification.cs passed in Edit and Play Mode.

- CheckAuthoring verified two connected pickups with unique IDs and continuous physics. The world pickups contain no arc renderers. Both equipped throw systems reference the same nested arc prefab.
- CheckRun exercised two aimed pickups, duplicate pickup rejection, a two-unit count, HUD selection, tool switching and an independent equipped visual. It compared all trajectory points for the same aim, checked blocked and paused release, broke a window through touch primary input, emptied the stack through mouse primary input and recovered a thrown brick.
- CheckSignal verified invalid and valid landing, focus gating, one consumed canister, a single projectile and smoke at the predicted endpoint.
- CheckResidentRegression verified shoulder attachment, release and throw velocity after separating bricks from rescue carrying.

The [inventory capture](../../Art/Props/ThrowableBrick/throw-arc.png) shows the selected fourth slot with two bricks and its trajectory after opening the window. The [canister reference](../../Art/Props/ThrowableBrick/signal-arc-reference.png) shows the original aim pose. The slot was inspected in 800 × 600 and 1280 × 720 captures. The Editor finished in Edit Mode with zero current console errors. CLI references used were unity-cli.md, common-workflows.md and playmode-verification-loop.md.

Physical keyboard, mouse and touchscreen playtesting and final tuning remain open. The local Obsidian progress file is excluded by the project’s existing Git ignore rule.
