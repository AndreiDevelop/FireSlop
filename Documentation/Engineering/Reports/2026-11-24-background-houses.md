# Background houses

2026-11-24 · Partial delivery of expansion feature 3, **Houses in the fog**.

## Saved content

`Assets/Prefabs/Environment/Street/NightStreet.prefab` now contains eleven additional connected instances of the existing `BackgroundBlockHouse.prefab`, grouped under `Distant Courtyard Houses`. Placement surrounds the courtyard at greater distances than the five existing background houses. Authored height variations reuse the existing exterior geometry and materials.

The nested `BackgroundGround.prefab` reuses the street mesh and material. Its surface is 3 cm below the current Main street surface to avoid coplanar overlap. The new houses meet this surface. It has no collider; no runtime scripts were added.

## Verification

Unity 6000.6.0f1, Pipeline 1.0.0, via Unity CLI `script eval` and prefab APIs. CLI references read: `unity-cli.md`, `common-workflows.md` and `playmode-verification-loop.md`.

- Reloaded the saved prefab contents: exactly one distant-house group and eleven connected house instances.
- Each house has valid materials and shaders, fits inside the background-ground bounds, and meets the ground within 1 mm.
- No pair of new house renderer bounds intersects. No house contains gameplay MonoBehaviours.
- Ground prefab connection is intact and has zero colliders.
- Final console status: no compilation failure and zero current console errors.

## Pending acceptance

The user restricted further work to prefabs while another task uses the Editor. Main fog settings were not changed. Its saved linear fog currently spans 10–20 m, which requires review against the distant silhouettes. Tune fog and inspect actual views from the street, windows and roof when the Editor is available. Saved-scene reload, startup/restart propagation and runtime duplicate checks remain unverified. Feature 3 is not marked complete.
