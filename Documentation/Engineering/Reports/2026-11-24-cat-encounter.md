# Cat Encounter and Rescue

Implemented 2026-11-24 in Unity 6000.6.0f1. Scope: expansion feature **1 — Cat jumps at the player's face**, including the requested pickup and Save-zone rescue after calming. [Approved expansion](../../../FireSlop/Documentation/GDD/Design/SinglePlayerExpansion.md).

## Behavior and ownership

Two connected instances of `Assets/Prefabs/Characters/Cat.prefab` are saved in Main: `cat-ground-hall` and `cat-upstairs-room`. Both now start in the rear apartments on floors one and two, outside the initial fire seeds. Startup preserves their authored positions; each apartment has a connected ApartmentPatrol prefab and saved Cat NavMesh data. Each prefab has a Zenject GameObjectContext and its own CatModel, injected into its Controller and View. A scene-level CatFaceModel prevents simultaneous face attachment.

A clear nearby sightline makes the cat turn toward the player and starts the three-second hiss and three red exclamation marks. Retreating beyond notice range or breaking sight hides the marker and stops hissing. Water calming plays a purr-meow cue once. Breaking sight cancels the warning. Existing ballistic hose packets and auto-spray targeting calm the cat before takeoff. Ignoring the warning produces an arcing jump and camera-relative attachment. E and the touch context button share the removal command, ahead of drone and aimed-object actions. Removing the cat animates it back to its takeoff point and leaves it calm. The animation preserves the imported model's authored rotation.

A calm cat uses the existing Carryable/RescueController flow. It can be picked up, put down or thrown. Save-zone delivery releases a carried cat into the world, clears the carried-item state and marks the cat rescued exactly once. A rescued cat cannot be picked up again. Cats and the resident implement IRescueTarget; the zone dispatches the same delivery operation to either. Resident delivery still ends the mission. Cat delivery leaves the resident objective and timer running, without a currency reward. State resets on a fresh run.

`Assets/SO/Cats/Cat.asset` owns provisional tuning: 3.8 m notice range, 3 s warning, 0.3 s accumulated water, 0.4 s jump/return, 0.45 m arc, 12 s attack cooldown and 3 kg mass. The hiss is a three-second synthesized mono WAV; the 1.8-second purr-meow is synthesized voiced audio. Both use compressed import and the existing effects-volume control. Final balance and sound choice need playtesting.

## Apartment movement and fire waiting

CatPatrolController uses the installed AI Navigation 2.0.14 package. The distinct Cat agent type has 0.22 m radius, 0.65 m height and 0.12 m climb; the Humanoid tuning is preserved. Two 3.6 × 2.4 × 3.5 m volume surfaces collect static physical room geometry, excluding movable furniture, with 0.07 m bake voxels. Their saved data is under `Assets/Settings/Navigation/`. No runtime bake, external links or roaming outside the assigned apartment is used. Cat rigidbodies remain kinematic while navigating; navigation disables for jumping, calming and carrying. Model and timers are local to each cat's Zenject context.

Four authored points define each loop. Initial roaming tuning is 0.75 m/s, 3 m/s² acceleration, 180°/s turns, 0.12 m arrival distance, 0.7 s pauses/rechecks and 0.55 m fire clearance. Active fire is checked along candidate routes and ahead of movement. With no safe route, the cat stops immediately, cancels warning/attack and waits for extinguishing. Clearing a route resumes movement. The hose's moving-target mask now includes the cats' water-target layer. The original static visual had no gait; the later Ginger cat update below adds it.

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

## Attack cooldown verification

Water calming and completed removal start a 12-second rescue window configured in `Assets/SO/Cats/Cat.asset`. The timer pauses while carried or gameplay input is disabled. Expiry resets accumulated water and returns a free cat to Idle; every subsequent attack requires a fresh three-second warning. Rescued cats never rearm. A released cat is made kinematic before its hostile trigger colliders are used, preventing a repeat attacker from falling through the floor. Outside the assigned apartment it can attack in place but does not navigate.

`CatVerification.RunCooldownChecks` passed threshold, carried protection, fresh water, repeated warning/jump, removal expiry and rescued-state checks. `CatDynamicNavigationVerification.CheckCooldown` passed actual pickup, carried timer pause, shared-action put-down, renewed warning, repeated jump/attachment, stable physical takeoff position, removal and final carried Save-zone rescue.

## Moving furniture verification

The five reusable furniture prefabs (Sofa, Armchair, CoffeeTable, Cabinet and Board) now contain authored box NavMeshObstacles; Main has 31 instances. The baked apartment surfaces exclude the movable-furniture layer, avoiding permanent holes at old furniture positions. Existing NavMesh assets were updated in place. `FurnitureNavigationView` applies stationary carving from `Assets/SO/Navigation/FurnitureNavigation.asset`: 0.1 m movement threshold and 0.3 s stationary delay. It adds no per-object Update loop. Cats also capsule-check physical obstructions immediately, using the 0.17 m radius and layer mask from Cat SO, while moving obstacles have not yet carved their new location. Furniture obstruction is distinct from fire trapping. After this change, the existing roaming/fire regression passed all eight reachable patrol points, movement by both cats within bounds, pause, surrounding-fire waiting without attack, and resumption after extinguishing. The ballistic-water/visual-warning/context-pickup/Save-zone regression also passed.

`CatDynamicNavigationVerification.CheckFurniture` passed reopening the old sofa footprint, immediate blocking before carving settles, carving its new footprint and reopening that temporary footprint after another move. `CheckWaterPush` passed an actual water impact moving the dynamic sofa; navigation does not take ownership of furniture physics. Reference: [NavMeshObstacle](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavMeshObstacle.html).

## Performance measurement

Measured in the running macOS Unity Editor using `CatNavigationPerformance.Sample`, with two cats, 31 furniture obstacles and advancing game frames. These are profiler-marker sample readings in milliseconds, not a platform-build benchmark.

| Fixture | Frames | NavMeshManager mean | NavMeshManager p95 | Cat.Patrol mean | Cat.Patrol p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| Before dynamic furniture | 120 | 0.0606 | 0.0776 | Not recorded | Not recorded |
| After, stationary furniture | 180 | 0.0818 | 0.1153 | 0.0427 | 0.0598 |
| After, three moving objects | 180 | 0.0953 | 0.1178 | 0.0430 | 0.0596 |

The moving fixture increased the native navigation marker by about 0.035 ms relative to the baseline. Native navigation plus the cat patrol marker averaged about 0.14 ms. Main Thread means were 15.74 ms before and 18.57–18.79 ms afterward; these whole-scene measurements include unrelated simulation and Editor work and cannot isolate navigation overhead. Device/WebGL performance remains unverified. Full runtime NavMesh rebuilds are not used; carving occurs after obstacles settle. No player build was created.

## Remaining checks

Physical phone/browser input, touch-layout visual inspection, other aspect ratios, drone delivery of a cat and final encounter/audio tuning remain unverified. Re-bake the apartment surfaces after changing static room geometry; furniture movement does not require a full runtime bake. The purr-meow is a synthesized cartoon cue, not a recorded cat. No player build was requested or created. The next expansion feature is **2 — Rescue three residents**.

## Reproduce

From the project root, run Unity CLI with `--project-path /Users/andrii/Projects/GametekaForge/FireSlop --caller plugin --skill programmer-plan-implementation` on every `unity command` call. Run `script run --file AgentScripts/CatVerification.cs --entry FireSlop.Verification.CatVerification.RunModelChecks` for model checks. In a fresh Play session, run the four CatPlayVerification methods in the order listed above; CheckJump pauses the Editor for capture and CheckRemoval resumes it. For the follow-up, run CatVerification.RunCooldownChecks in Edit Mode and CatDynamicNavigationVerification.CheckCooldown in fresh Play Mode. CheckFurniture and CheckWaterPush use the same script. Sample profiler markers with CatNavigationPerformance.Sample(label, frames, moveFurniture). Stop Play Mode afterward. Scripts are outside Assets and do not ship.

## Ginger cat model and animation update

2026-11-24: replaced the gameplay Cat prefab's visual with `Assets/Models/Characters/CatGinger.fbx`, authored from the supplied Meshy Blender file. [Production details and five Unity previews](../../Art/ModelProduction/README.md#ginger-cat-animations). The original download and legacy static model were retained. Gameplay IDs, colliders, saved placement, encounter tuning and connected Cat scene instances were preserved. The saved prefab includes an explicit idle pose, so it is readable outside Play Mode.

CatAnimationView owns the serialized Animator, NavMeshAgent, CatCarryable and stun-stars references. Zenject injects the existing CatModel, CatConfigSO and MissionInputModel. Actual agent velocity selects and scales the in-place Walk cycle; warning stops the gait. Jumping/Returning use extended attack paws, Attached uses FaceGrab, and Settled uses Stunned with a circling head and three orbiting stars. Carrying and Rescued use Idle without stars. Gameplay input suspension freezes animation. No runtime hierarchy, skeleton, star geometry or permanent layout is generated.

Verification through Unity CLI, `--caller plugin --skill meshy-unity-assets`:

- Automatic C# compilation completed; the new View type loaded successfully.
- `CatVerification.RunModelChecks` passed existing warning/water/face ownership/removal/rescue/action-priority checks.
- `CatAnimationVerification.CheckSavedPrefab` passed: connected model, one skinned body, five looping clips, resolved animation bindings, no root motion, hidden default stars and correct CatView visual reference.
- `CatAnimationVerification.RunIsolated` passed in advancing Play Mode: real NavMesh movement selects Walk; foreleg and tail move; warning switches to Idle; JumpAttack/FaceGrab/return select distinct poses; suspended input pauses; stunned head and star orbit animate; carrying and rescue hide stars; exactly two CatAnimationViews remain. This fixture manually injects the existing authored cat because scene-wide injection is currently blocked. Carry attachment/delivery use the existing Carryable methods in the isolated fixture; it does not claim a complete player-input rescue test.
- Five 640 × 640 Unity preview renders were inspected for material assignment and pose/skin deformation. Blender poses were also inspected. No player build was made.

Full-scene integration could not be verified in this session: concurrent project changes leave missing leaderboard references (`LeaderboardView` and leaderboard settings in the wardrobe installer), which abort scene-wide Zenject injection and cause downstream uninitialized gameplay components. Existing duplicate `_carrier` field messages on ResidentView/CatCarryable were also observed. Those unrelated systems were not changed by this task. The failed full-scene run was stopped; the previous Play start scene and background/tick overrides were restored.
