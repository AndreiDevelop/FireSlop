# Rescue zone and immediate victory

Updated: 2026-11-17. Scope: implementation step 09. Status: implementation saved; repeated Restart verified with the GPU Resident Drawer workaround below.

The connected RescueZone prefab in Main accepts the authored resident. Free delivery uses the resident capsule; carrying checks the shoulder position inside the authored box because the carried capsule is disabled. MissionModel owns the one-shot transition and completion time. Victory disables gameplay input and exposes the authored result panel in the HUD prefab. Restart requests a fresh Main scene and additive HUD. Tool-only prototype scenes can run without mission bindings.

## Verified

- Model checks: reject rescue before start and wrong IDs; accept the resident once; freeze elapsed time; a new model starts at zero.
- Live Editor frames advanced. Free resident delivery and carrying without release both produced victory.
- Empty player and furniture did not complete the mission. Six fire sources remained present; complete extinguishing is not required.
- Victory cancelled an active hose and rejected movement, look, tool selection and repeated completion.
- Main had one zone and a serialized resident reference. The HUD prefab contained the saved result component.
- Following Restart interaction, Main/HUD had one resident, one zone, one EventSystem, closed cabinet, free hands and enabled input. The initial automated repeatability/timer-reset assertion was interrupted by the native crash; the follow-up regression below passed.
- Result layout was inspected at actual Game View sizes: [640×360](../../Art/UI/MissionResult/result-640x360.png), [800×600](../../Art/UI/MissionResult/result-800x600.png), and [1920×1080](../../Art/UI/MissionResult/result-1920x1080.png). Text and rounded panels fit.
- No managed errors or compilation failures were reported in the last successful console check. Git diff whitespace check passed.

## Restart crash workaround and follow-up verification

The macOS crash report `Unity-2026-11-17-125832.ips` records SIGSEGV in `ParallelFilterChangedInstancesAndCreateScriptingArray` / `ExecuteJob_NoScriptingArray`; the Editor log also includes `ObjectDispatcher.DispatchTypeChangesAndClear`. The active PC render pipeline had GPU Resident Drawer enabled. The installed render pipeline's GPU-driven `WorldProcessor` uses this object-dispatch path to track changes.

Disabled GPU Resident Drawer in [PC_RPAsset](../../../Assets/Settings/PC_RPAsset.asset) through the Unity asset API. Mobile already had it disabled. This is a rendering workaround for the observed native crash, not an engine patch; the precise native lifetime fault is not established. Unity and package versions are unchanged. GPU-driven batching is unavailable in this profile until the workaround is removed; performance impact has not been benchmarked. Unity also serialized its current default fields while saving the asset.

Live Unity CLI regression on Unity 6000.6.0f1 / Metal passed six consecutive rescue/restart cycles (three carried, three released). The check moved the resident into the authored rescue volume, allowed actual physics/game frames to complete the mission, and invoked the authored Restart button's `onClick` twice per cycle. Assertions covered advancing frames and timer, visible results, frozen completion time, a new active mission with reset timer, reloaded Main/HUD, closed cabinet, free hands, hidden results, one resident/zone/EventSystem and one ragdoll including inactive objects. Console status after the run reported zero errors/warnings and no compilation failure. Play Mode was stopped after verification.

## Remaining checks

Full reset coverage remains in steps 16–17. Physical phone, portrait/safe-area coverage and browser verification were not performed. No player build was created. Re-enabling GPU Resident Drawer requires a separately verified engine fix and repeating the rescue/restart regression.

TMP Essentials were imported from the installed official uGUI package after the standard importer failed to materialize the files. Package GUIDs were retained. Result labels use a locally authored TMP font asset from the bundled Liberation Sans font.
