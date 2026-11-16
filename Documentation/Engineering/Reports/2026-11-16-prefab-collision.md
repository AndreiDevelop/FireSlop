# Prefab collision validation

Date: 2026-11-16. Environment: Unity 6000.6.0f1, Edit Mode, isolated preview physics scenes.

## Scope and result

28 prefab assets updated through Unity Editor APIs. Existing geometry, scene placement and nested prefab connections preserved. No runtime generator or player controller added.

- Saved prefab reload: 579 collider instances inspected across prefab assets and their nested assemblies; only Box, Capsule and Sphere Colliders, enabled and non-trigger. All nested prefab links connected. Counts include reused instances, not unique components.
- Main inherited 505 collider instances without scene overrides for this change.
- Doorway and window wall openings: center ray clear; side and lintel rays hit.
- Wall sections, breakable-wall placeholder, floor slab and stair landing: collision rays hit.
- Ramp: eight sampled positions on a continuous 32-degree surface.
- Temporary CharacterController: ascent/descent and both landing joins passed on an isolated flight. Height 1.86 m, radius 0.34 m, slope limit 45 degrees, step offset 0.2 m, skin width 0.03 m.
- All six BaseHouse flights passed ascent with the same controller, with the complete house walls, slabs and railings present.

## Stair railing follow-up

The sloped stair railing now uses six fitted boxes, one for the handrail and one per post. All 48 corners were within the visual bounds of the railing, with a 0.1 mm tolerance. The CharacterController probe again climbed all six flights in the assembled house.

## Limits

The player has no movement implementation yet. Recheck controller-specific slope, grounding and animation behavior when it is implemented. Box approximations simplify furniture, drone guards and fractured fragments. No player build or runtime destruction test was performed.
