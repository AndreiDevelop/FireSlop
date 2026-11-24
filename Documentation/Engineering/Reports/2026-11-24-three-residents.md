# Three-resident rescue

Updated: 2026-11-24. Scope: [single-player expansion feature 2](../../../FireSlop/Documentation/GDD/Design/SinglePlayerExpansion.md). Status: implementation saved; live gameplay acceptance pending.

## Implemented

- Main contains three connected `RescueResident.prefab` instances. The original resident ID remains `main-resident-01`; the added IDs are `main-resident-02` and `main-resident-03`. Each instance owns its visual, pickup body, water-knockdown components and carry rig.
- Each new mission selects three of the five existing cabinets without replacement. All floors remain eligible, including combinations with more than one resident on a floor. The former first-run preference and its PlayerPrefs flag are no longer used. Scene placement remains visible in Edit Mode; the approved random hiding selection only repositions existing residents during initialization.
- Opening an occupied cabinet reveals only its resident. Pickup checks that resident's cabinet, release state and rescue state through the existing carry receiver contract. Occupied movable cabinets remain stationary until their resident is picked up.
- MissionModel owns eligible IDs, reveal state, rescued IDs, progress and time. Repeated contact is rejected by ID. The first two rescues keep the mission active; the third freezes time and raises the existing win event once. Cats remain outside the resident objective.
- Exit and trampoline retain their shared rescue receiver path. Accepted resident delivery frees the shoulder, stows the carry rig and prevents subsequent pickup. The existing completion callback, including its configured wardrobe reward, runs once on mission completion.
- The existing objective arrow skips rescued residents, prefers a revealed pending resident and points to the exit while carrying any mission resident. Compact authored text displays progress from `0/3` through `2/3`; the result title is `3/3 residents rescued!`.

## Checks completed

- Unity 6000.6.0f1 / Pipeline 1.0.0: runtime source compilation completed with zero compile errors. Later source edits only adjusted objective copy and spacing; no second disruptive recompile was requested while the shared Editor was occupied.
- `ResidentRescueVerification.CheckModel` passed: independent reveal, immutable input IDs, rejection before start and for unknown IDs, one rescue per resident, active mission after rescues one and two, a single final win, frozen time and fresh-model reset. Duplicate authored IDs are rejected.
- Unity API inspection immediately after authoring found three residents with distinct IDs at saved positions, all connected to the shared prefab. Imported prefab inspection confirmed pickup collider, Rigidbody, visual, carry rig and knockdown references. Saved Main source inspection confirmed three prefab instances and three non-null references in both the installer and carrier arrays.
- TMP preferred-size checks for the compact objective strings fit the existing 182 × 38 label at its authored 18-point size. The result title fits its authored anchors at 640 × 360, 800 × 600 and 1920 × 1080 without Auto Size. These are layout measurements, not rendered visual acceptance.
- Scoped C# whitespace checks passed. Parallel wardrobe changes are present in the working tree and were preserved.

## Pending acceptance

The user requested continuing without Play verification while another task uses the Editor. No three-resident gameplay run is claimed as passing.

Run `AgentScripts/ResidentRescueVerification.cs` through Unity CLI `script run` with the explicit FireSlop project path and caller/skill flags. `CheckAuthoring` requires Main open in Edit Mode. In a fresh Main Play session, `CheckRun` exercises independent cabinet reveal, pickup, the touch throw command, trampoline contact, carried exit delivery, intermediate HUD progress, duplicate rejection and final time freeze. Then `CheckRestart` invokes the authored Restart button and checks the new model, closed cabinets, fresh selection, three residents and three carry rigs. The existing cat resident regression was updated for the multi-resident API.

Also verify mouse/keyboard and touch context dispatch, carrying through doors and onto the drone after a partial rescue, repeated delivery to both receivers, multiple random restarts including a shared-floor placement, and rendered HUD/result appearance. Confirm partial rescues retain movement/tools and only the third disables input. Physical-device and browser acceptance remain open. No player build was created.

The verification run completes a mission and therefore exercises the existing persistent wardrobe reward. Use a disposable test profile or preserve and restore that profile's save value before running it.
