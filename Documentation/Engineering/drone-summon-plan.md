# Drone summon implementation plan

Updated 2026-11-18. Owner TBD. Stage 21 is required for the solo demo (P1). All tasks below are pending. Follow the [approved summon rules](../../FireSlop/Documentation/GDD/GDD.md#smoke-canister-summon) and [mission plan](../../FireSlop/Documentation/GDD/ImplementationPlan.md). Stages 04, 08, 09, 12 and 15 supply the tool slots, carrying, victory, roof route and piloted drone.

### Start and restart

- [ ] **21.1** Move the single connected drone instance to an authored offstage position behind surrounding buildings. Preserve its stable ID and references. Separate the playable flight envelope from the offstage start before moving the instance. Check roof, courtyard and rescue-zone reachability against the limits in [DroneController](../../Assets/Scripts/Drone/DroneController.cs).
- [ ] **21.2** Initialize one canister and an awaiting-signal drone on every run. Gate boarding, rotor sound and empty-drone descent until their respective flight states. In Edit Mode verify the prefab connection. In Play Mode walk across the former parking spot and check that it is clear and has no boarding prompt.

### Aim and throw

- [ ] **21.3** Extend the tool selection model, first-person item and authored desktop/touch HUD with slot 3. Display the red canister and its spent state. Use a fresh action press to throw. Carrying keeps the existing resident actions. Putting the resident down enables canister use.
- [ ] **21.4** Predict the arc against scene collision and draw the landing marker using the drone footprint. Check an outdoor endpoint, supported footprint, overhead clearance and a clear approach. Test roof edges, obstacles, indoor floors and intact versus cleared windows. Aiming through an open window must work from indoors.
- [ ] **21.5** Recheck the target when committing the throw. An invalid throw keeps the charge. A valid press spends it once, sends the canister along the previewed arc and starts red smoke at the marked destination on impact. Switching slots or losing focus while aiming clears the preview. Repeated input cannot dispatch a second drone.

### Arrival and pilot handoff

- [ ] **21.6** Fly the existing drone from behind the buildings into view, approach the confirmed point and descend onto it. Tune approach speed and descent in configuration. Check the silhouette from the roof and courtyard, roof-edge clearance and agreement between the preview and final deck position. If someone occupies the landing footprint during approach, hold clear and finish once it is free.
- [ ] **21.7** Keep the firefighter controllable during arrival so they can pick the resident back up. On touchdown enable the existing deck interaction and hand off to stage 15. Verify boarding with the resident, manual flight and one victory on delivery to the existing rescue zone. An empty landing or boarding alone must leave the mission active.

### Acceptance and reset

- [ ] **21.8** On desktop and touch run both routes from a fresh mission. First carry the resident to the roof, put them down, signal, pick them up during arrival and fly them out. Then call from the courtyard before entry and pilot to the window. Check marker readability and whether a new player can locate and board the landed drone without prompting. Tune timing from these runs.
- [ ] **21.9** Restart while aiming, after release, during approach and after rescue. Clear the marker, projectile, smoke, pending arrival, pilot state and camera. Confirm one charged slot, one offstage drone and an empty courtyard each time. Verify pedestrian and trampoline rescue still work with the charge unused.

2026-11-18 — Added stage 21 to replace the parked courtyard start. Earlier flight checks are recorded in the [stage 15 report](Reports/2026-11-17-drone-flight.md).
