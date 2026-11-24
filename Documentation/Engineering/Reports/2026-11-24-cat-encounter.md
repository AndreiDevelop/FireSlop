# Cat Encounter and Rescue

Implemented 2026-11-24 in Unity 6000.6.0f1. Scope: expansion feature **1 — Cat jumps at the player's face**, including the requested pickup and Save-zone rescue after calming. [Approved expansion](../../../FireSlop/Documentation/GDD/Design/SinglePlayerExpansion.md).

## Behavior and ownership

Two connected instances of `Assets/Prefabs/Characters/Cat.prefab` are saved in Main: `cat-ground-hall` and `cat-upstairs-room`. Both now start in the rear apartments on floors one and two, outside the initial fire seeds. Startup preserves their authored positions; each apartment has a connected ApartmentPatrol prefab and saved Cat NavMesh data. Each prefab has a Zenject GameObjectContext and its own CatModel, injected into its Controller and View. A scene-level CatFaceModel prevents simultaneous face attachment.

A clear nearby sightline makes the cat turn toward the player and starts the three-second hiss and three red exclamation marks. Retreating beyond notice range or breaking sight hides the marker and stops hissing. Water calming plays a purr-meow cue once. Breaking sight cancels the warning. Existing ballistic hose packets and auto-spray targeting calm the cat before takeoff. Ignoring the warning produces an arcing jump and camera-relative attachment. E and the touch context button share the removal command, ahead of drone and aimed-object actions. Removing the cat animates it back to its takeoff point and leaves it calm. The animation preserves the imported model's authored rotation.

A calm cat uses the existing Carryable/RescueController flow. It can be picked up, put down or thrown. Save-zone delivery releases a carried cat into the world, clears the carried-item state and marks the cat rescued exactly once. A rescued cat cannot be picked up again. Cats and the resident implement IRescueTarget; the zone dispatches the same delivery operation to either. Resident delivery still ends the mission. Cat delivery leaves the resident objective and timer running, without a currency reward. State resets on a fresh run.

`Assets/SO/Cats/Cat.asset` owns provisional tuning: 3.8 m notice range, 3 s warning, 0.3 s accumulated water, 0.4 s jump/return, 0.45 m arc and 3 kg mass. The hiss is a three-second synthesized mono WAV; the 1.8-second purr-meow is synthesized voiced audio. Both use compressed import and the existing effects-volume control. Final balance and sound choice need playtesting.

## Apartment movement and fire waiting

CatPatrolController uses the installed AI Navigation 2.0.14 package. The distinct Cat agent type has 0.22 m radius, 0.65 m height and 0.12 m climb; the Humanoid tuning is preserved. Two 3.6 × 2.4 × 3.5 m volume surfaces collect physical room geometry, with 0.07 m bake voxels. Their saved data is under `Assets/Settings/Navigation/`. No runtime bake, external links or roaming outside the assigned apartment is used. Cat rigidbodies remain kinematic while navigating; navigation disables for jumping, calming and carrying. Model and timers are local to each cat's Zenject context.

Four authored points define each loop. Initial roaming tuning is 0.75 m/s, 3 m/s² acceleration, 180°/s turns, 0.12 m arrival distance, 0.7 s pauses/rechecks and 0.55 m fire clearance. Active fire is checked along candidate routes and ahead of movement. With no safe route, the cat stops immediately, cancels warning/attack and waits for extinguishing. Clearing a route resumes movement. The hose's moving-target mask now includes the cats' water-target layer. There is no gait animation.

References consulted: [NavMesh Surface](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavMeshSurface.html), [mixing agents and rigidbodies](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/MixingComponents.html), [patrol](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavAgentPatrol.html).

## Verification

- Unity CLI recompile: zero errors and warnings. Final Console check: zero errors.
- `AgentScripts/CatVerification.cs`, `RunModelChecks`: warning timing/cancellation, independent state, water threshold and idempotence, exclusive face reservation, removal and rescue retries, drone action priority, mission-action gating and complete HUD catalog.
- `AgentScripts/CatPlayVerification.cs`, `CheckWaterAndRescue`: apartment placement, three red markers, warning still active before the three-second deadline, facing the player, retreat and sight cancellation, actual ballistic hose calming, purr-meow AudioSource playback, independent second cat, normal context pickup, carried Save-zone delivery, freed hands and no repeated rescue or premature victory.
- `AgentScripts/CatPatrolVerification.cs`, `CheckRoamingAndFire`: all eight authored patrol points reachable, both cats visibly moving within apartment bounds, pause, surrounding active fire stops movement and attacks even with the player close, and actual extinguishing resumes patrol.
- `CheckJump`: independent second-cat warning, suspended-input pause, timed jump, face attachment and Remove cat prompt.
- `CheckRemoval`: normal shared interaction command, exclusive reservation released, return animation completed and rescue pickup available.
- `CheckResidentRegression`: resident pickup, carried Save-zone delivery, victory and duplicate rejection through the generalized zone.
- The Editor player loop advanced during testing. Play Mode ran Main and additive HUD from Intro. The checks temporarily position actors and call the same interaction commands used by UI; they do not simulate physical keyboard or touch events.
- A paused 1280 × 720 Game-view capture confirms the cat faces the player, partially obscures the view and leaves the desktop removal prompt readable: [attached cat](../../Art/Characters/Cat/Validation/attached-desktop.png). The courtyard positioning in this capture is a test fixture, not saved level placement.
- Saved Main was reopened to inspect two connected prefab instances, distinct IDs and edit-time visibility. No runtime duplicate generation is present.

The dynamic TMP font cache populated by these tests was restored to its baseline, preserving the original kerning data. Optional removal of stale prefab overrides was rejected by automatic approval review; those overrides were retained, and the active Carryable IDs remain verified.

Warning appearance was inspected at 1280 × 720 inside the actual ground-floor apartment: [three red marks and facing](../../Art/Characters/Cat/Validation/apartment-warning.png). The indicator is authored under Cat/Warning/Body, reuses the existing static TMP font, has fixed font sizing and receives no raycasts.

## Remaining checks

Physical phone/browser input, touch-layout visual inspection, other aspect ratios, runtime displacement of room furniture, drone delivery of a cat and final encounter/audio tuning remain unverified. Re-bake the apartment surfaces after changing the room geometry. The purr-meow is a synthesized cartoon cue, not a recorded cat. No player build was requested or created. The next expansion feature is **2 — Rescue three residents**.

## Reproduce

From the project root, run Unity CLI with `--project-path /Users/andrii/Projects/GametekaForge/FireSlop --caller plugin --skill programmer-plan-implementation` on every `unity command` call. Run `script run --file AgentScripts/CatVerification.cs --entry FireSlop.Verification.CatVerification.RunModelChecks` for model checks. In a fresh Play session, run the four CatPlayVerification methods in the order listed above; CheckJump pauses the Editor for capture and CheckRemoval resumes it. Stop Play Mode afterward. Scripts are outside Assets and do not ship.
