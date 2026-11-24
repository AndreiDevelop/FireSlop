# Drone landing preview

Expansion feature 8 was implemented and verified in Unity 6000.6.0f1 on 2026-11-24. See the [approved scope](../../../FireSlop/Documentation/GDD/Design/SinglePlayerExpansion.md).

## Behavior and assets

A valid signal-canister aim shows a translucent, full-size drone at the predicted touchdown position with the arriving drone's orientation. The original green frame is removed. The existing trajectory, landing eligibility, charge consumption and approach remain authoritative.

The [preview prefab](../../../Assets/Prefabs/Modules/Drone/LandingPreview.prefab) references the same 21 meshes and preserves their transforms, scale and ground pivot. Its authored transparent unlit material is [LandingPreview](../../../Assets/Materials/Drone/LandingPreview.mat). It has no colliders, scripts, audio or shadows. SignalThrow contains one connected nested instance, initially hidden. SignalThrowController updates its pose and visibility without spawning a second gameplay drone or changing materials at runtime.

The preview hides for invalid landing, tool switching, pause, focus loss, component disable and successful release. Reopening valid aim restores it without spending a charge.

## Verification

[DroneLandingPreviewVerification](../../../AgentScripts/DroneLandingPreviewVerification.cs) runs through Unity CLI script run, outside the runtime assembly:

- CheckAuthoring passed: mesh identity and transforms, transparent material/depth settings, no physics/audio/gameplay components, connected nested instance and removal of the old frame.
- CheckAim passed in a fresh mission: valid courtyard pose, invalid trajectory, pause, focus loss, tool switch, disable/re-enable and unchanged canister quantity.
- CheckRoof passed after restarting: restored canister and a valid preview above roof height.
- CheckLanding passed separately in the courtyard and on the roof: release through the shared touch primary action hid the ghost immediately; repeated mouse primary input left one drone; actual touchdown matched the preview within 0.01 m and 0.01 degrees.
- Frame advancement was confirmed with job wait before runtime checks. Recompilation completed without errors. Final console ground truth reported no compilation failure or current console errors.

The [roof capture](../../Art/Props/Drone/landing-preview-roof.png) was visually inspected at 1280 × 720. It is an Editor camera capture with scripted test placement. At close aiming distances, the full-size drone may extend beyond the top of the viewport. Physical keyboard/touchscreen playtesting remains open.

CLI reference pages used: unity-cli.md, integration-advanced.md, common-workflows.md and playmode-verification-loop.md. The sandbox initially prevented the CLI from reaching the local Editor; the same CLI connected with local access. No player build was created.
