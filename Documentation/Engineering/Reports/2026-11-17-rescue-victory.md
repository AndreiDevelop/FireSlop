# Rescue zone and immediate victory

Updated: 2026-11-17. Scope: implementation step 09. Status: implementation saved; repeatable Restart verification remains open.

The connected RescueZone prefab in Main accepts the authored resident. Free delivery uses the resident capsule; carrying checks the shoulder position inside the authored box because the carried capsule is disabled. MissionModel owns the one-shot transition and completion time. Victory disables gameplay input and exposes the authored result panel in the HUD prefab. Restart requests a fresh Main scene and additive HUD. Tool-only prototype scenes can run without mission bindings.

## Verified

- Model checks: reject rescue before start and wrong IDs; accept the resident once; freeze elapsed time; a new model starts at zero.
- Live Editor frames advanced. Free resident delivery and carrying without release both produced victory.
- Empty player and furniture did not complete the mission. Six fire sources remained present; complete extinguishing is not required.
- Victory cancelled an active hose and rejected movement, look, tool selection and repeated completion.
- Main had one zone and a serialized resident reference. The HUD prefab contained the saved result component.
- Following Restart interaction, Main/HUD had one resident, one zone, one EventSystem, closed cabinet, free hands and enabled input. The final automated repeatability/timer-reset assertion did not finish.
- Result layout was inspected at actual Game View sizes: [640×360](../../Art/UI/MissionResult/result-640x360.png), [800×600](../../Art/UI/MissionResult/result-800x600.png), and [1920×1080](../../Art/UI/MissionResult/result-1920x1080.png). Text and rounded panels fit.
- No managed errors or compilation failures were reported in the last successful console check. Git diff whitespace check passed.

## Remaining checks

Unity 6000.6.0f1 crashed natively during verification, with ObjectDispatcher / ParallelFilterChangedInstancesAndCreateScriptingArray in the native stack. The crash root cause is not established. Repeat Restart acceptance after resolving the Editor instability; do not treat step 09.5 as complete. Full reset coverage remains in steps 16–17. Physical phone, portrait/safe-area coverage and browser verification were not performed. No player build was created.

TMP Essentials were imported from the installed official uGUI package after the standard importer failed to materialize the files. Package GUIDs were retained. Result labels use a locally authored TMP font asset from the bundled Liberation Sans font.
