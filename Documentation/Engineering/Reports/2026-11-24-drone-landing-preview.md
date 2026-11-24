# Drone landing preview

Expansion feature 8 was implemented and verified in Unity 6000.6.0f1 on 2026-11-24. See the [approved scope](../../../FireSlop/Documentation/GDD/Design/SinglePlayerExpansion.md).

## Behavior and assets

A valid signal-canister aim shows a translucent, full-size drone at the predicted touchdown position with the arriving drone's orientation. The original green frame is removed. The existing trajectory, landing eligibility, charge consumption and approach remain authoritative.

The [preview prefab](../../../Assets/Prefabs/Modules/Drone/LandingPreview.prefab) references the same 21 meshes and preserves their transforms, scale and ground pivot. Its authored transparent unlit material is [LandingPreview](../../../Assets/Materials/Drone/LandingPreview.mat). It has no colliders, scripts, audio or shadows. SignalThrow contains one connected nested instance, initially hidden. SignalThrowController updates its pose and visibility without spawning a second gameplay drone or changing materials at runtime.

The preview hides for invalid landing, tool switching, pause, focus loss, component disable and successful release. Reopening valid aim restores it without spending a charge.

## Throw availability and crosshair follow-up

Both throwable tools use the shared ThrowArc prefab's authored green and red colors. SignalThrowController uses its existing landing validation for both color and release. BrickThrowController uses the stored brick's CanLaunch check for both color and release. A blocked brick now keeps its arc visible in red instead of hiding it.

ThrowArcView reports visibility through a scene-scoped ThrowPreviewModel with independent ownership per arc. AimView observes its events and DroneModel; it hides the existing crosshair Body while any arc is visible or the drone is piloted. Disabling or unloading an arc releases its ownership. Colors remain authored in the prefab, with no runtime material creation or global object searches.

Unity CLI runtime checks passed for both tools' valid/invalid colors, rejected releases preserving inventory, successful touch canister and mouse brick releases, crosshair restoration, tool switching, pause/resume, focus loss, component disable/re-enable and pilot-mode transitions. A separate model check verified independent owners, idempotent updates and one visibility event per aggregate transition. Recompilation and current Console ground truth passed without errors. Physical-device testing remains open.

The updated [valid roof aim](../../Art/Props/Drone/landing-preview-roof.png) and [blocked aim](../../Art/Props/Drone/throw-arc-blocked.png) were inspected at 1280 × 720: green/red arcs, no crosshair while aiming, and no drone ghost at the invalid destination.

## Verification

[DroneLandingPreviewVerification](../../../AgentScripts/DroneLandingPreviewVerification.cs) runs through Unity CLI script run, outside the runtime assembly:

- CheckAuthoring passed: mesh identity and transforms, transparent material/depth settings, no physics/audio/gameplay components, connected nested instance and removal of the old frame.
- CheckAim passed in a fresh mission: valid courtyard pose, invalid trajectory, pause, focus loss, tool switch, disable/re-enable and unchanged canister quantity.
- CheckRoof passed after restarting: restored canister and a valid preview above roof height.
- CheckLanding passed separately in the courtyard and on the roof: release through the shared touch primary action hid the ghost immediately; repeated mouse primary input left one drone; actual touchdown matched the preview within 0.01 m and 0.01 degrees.
- Frame advancement was confirmed with job wait before runtime checks. Recompilation completed without errors. Final console ground truth reported no compilation failure or current console errors.

The [roof capture](../../Art/Props/Drone/landing-preview-roof.png) was visually inspected at 1280 × 720. It is an Editor camera capture with scripted test placement. At close aiming distances, the full-size drone may extend beyond the top of the viewport. Physical keyboard/touchscreen playtesting remains open.

CLI reference pages used: unity-cli.md, integration-advanced.md, common-workflows.md and playmode-verification-loop.md. The sandbox initially prevented the CLI from reaching the local Editor; the same CLI connected with local access. No player build was created.
