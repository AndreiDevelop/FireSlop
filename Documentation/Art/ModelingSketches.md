# Modeling Sketches

Status: Draft for review. Updated: 2026-11-16.

Eleven color concept sheets for modeling, generated with the built-in image_gen tool. They establish silhouettes and broad construction, not dimensionally exact CAD views, final textures or verified gameplay clearances. Final scale and collision geometry follow the playable blockout.

Sources: [Obsidian reference canvas](../../FireSlop/FireSlop/Consepts.canvas), [authoritative GDD](../../FireSlop/Documentation/GDD/GDD.md), [art guidelines](ART_DIRECTION_GUIDELINES.md). The supplied screenshots are visual references, not production textures. Sledgehammer and drone are new proposals in the same visual family.

## Sheets

### Resident

![Resident modeling reference](Characters/Resident/modeling-sheet.png)

Oversized eyes and head, hoodie and short limbs. Resident is modeled in a T-pose without a skeleton, as requested.

[Image](Characters/Resident/modeling-sheet.png) · [Generation prompt](Characters/Resident/prompt.json)

![Resident Blender model](Characters/Resident/model-preview.png)

[Blender source](Characters/Resident/source.blend) · [FBX](../../Assets/Models/Characters/Resident.fbx) · 2,248 triangles.

### Sledgehammer

![Sledgehammer modeling reference](Props/Sledgehammer/modeling-sheet.png)

Wooden shaft with yellow grip wrapping, as requested. Keep the symmetrical steel head and simple bevels.

[Image](Props/Sledgehammer/modeling-sheet.png) · [Generation prompt](Props/Sledgehammer/prompt.json)

![Sledgehammer Blender model](Props/Sledgehammer/model-preview.png)

[Blender source](Props/Sledgehammer/source.blend) · [FBX](../../Assets/Models/Props/Sledgehammer.fbx) · 296 triangles.

### Fire Hose

![Fire Hose modeling reference](Props/Hose/modeling-sheet.png)

Separate nozzle, bail handle, coupling and short hose. The hose continues beyond the illustrated section; this is not a closed solid tail.

[Image](Props/Hose/modeling-sheet.png) · [Generation prompt](Props/Hose/prompt.json)

![Hose Blender model](Props/Hose/model-preview.png)

[Blender source](Props/Hose/source.blend) · [FBX](../../Assets/Models/Props/Hose.fbx) · 420 triangles.

### Sofa

![Sofa modeling reference](Props/Sofa/modeling-sheet.png)

Three seat and three back cushions, broad arms and short feet. Match the armchair family.

[Image](Props/Sofa/modeling-sheet.png) · [Generation prompt](Props/Sofa/prompt.json)

![Sofa Blender model](Props/Sofa/model-preview.png)

[Blender source](Props/Sofa/source.blend) · [FBX](../../Assets/Models/Props/Sofa.fbx) · 616 triangles.

### Armchair

![Armchair modeling reference](Props/Armchair/modeling-sheet.png)

One seat and one back cushion. Match sofa bevels, upholstery and feet.

[Image](Props/Armchair/modeling-sheet.png) · [Generation prompt](Props/Armchair/prompt.json)

![Armchair Blender model](Props/Armchair/model-preview.png)

[Blender source](Props/Armchair/source.blend) · [FBX](../../Assets/Models/Props/Armchair.fbx) · 440 triangles.

### Coffee Table

![Coffee Table modeling reference](Props/CoffeeTable/modeling-sheet.png)

Plain low tabletop and four legs; keep its surface free of decorative props.

[Image](Props/CoffeeTable/modeling-sheet.png) · [Generation prompt](Props/CoffeeTable/prompt.json)

![CoffeeTable Blender model](Props/CoffeeTable/model-preview.png)

[Blender source](Props/CoffeeTable/source.blend) · [FBX](../../Assets/Models/Props/CoffeeTable.fbx) · 396 triangles.

### Breakable Door

![Breakable Door modeling reference](Props/Door/modeling-sheet.png)

Matching intact and pre-cut four-panel doors, with a separate frame. The pre-cut leaf contains 20 separately pivoted wooden fragments; impact behavior is not implemented.

[Image](Props/Door/modeling-sheet.png) · [Generation prompt](Props/Door/prompt.json)

![Door Blender model](Props/Door/model-preview.png)

[Blender source](Props/Door/source.blend) · [FBX](../../Assets/Models/Props/Door.fbx) · 1,096 triangles.

![Door fragments](Props/Door/fractured-exploded-preview.png)

[Fractured Blender source](Props/Door/fractured.blend) · [Fractured prefab](../../Assets/Prefabs/Props/DoorFractured.prefab)

### Rescue Drone

![Rescue Drone modeling reference](Vehicles/RescueDrone/modeling-sheet.png)

Four guarded rotors, landing skids and open standing platform with restraint. Fit the actual firefighter and carried resident under the hull before finalizing size; the pale scale figure is illustrative, not a clearance proof.

[Image](Vehicles/RescueDrone/modeling-sheet.png) · [Generation prompt](Vehicles/RescueDrone/prompt.json)

![RescueDrone Blender model](Vehicles/RescueDrone/model-preview.png)

[Blender source](Vehicles/RescueDrone/source.blend) · [FBX](../../Assets/Models/RescueDrone.fbx) · 2,000 triangles.

![Drone modular assembly](Vehicles/RescueDrone/exploded-preview.png)

Eight reusable module types form the 21-part connected prefab.

### Base House

![Base House modeling reference](Environments/BaseHouse/modeling-sheet.png)

Three floors with an intact roof, rear return stairs, corridor and a basic two-room apartment per floor. Six front bays span 12 m; footprint depth is 10 m. Windows, walls, doorways, floors, landings and railings are independent modules. The Unity assembly also includes connected furniture and door prefabs. Replaceable wall slots use 2 × 2.8 × 0.12 m placeholders with bottom-center pivots.

![House interior in Unity](ModelProduction/UnityPreviews/BaseHouse-interior.png)

[Image](Environments/BaseHouse/modeling-sheet.png) · [Generation prompt](Environments/BaseHouse/prompt.json)

![BaseHouse Blender model](Environments/BaseHouse/model-preview.png)

[Blender source](Environments/BaseHouse/source.blend) · [FBX](../../Assets/Models/BaseHouse.fbx) · 20,532 triangles before furniture.

### Breakable Wall Damage

![Oversized brick wall intact, with one brick removed, and with a larger opening](Environments/BaseHouse/BreakableWall/brick-wall-reference.png)

The brick infill has seven staggered courses. Full bricks are 80 × 40 cm, with half bricks at alternating edges. Each struck brick splits into three pieces and puffs out a small cloud of dust. The rest of the courses stay in place.

[Generation prompt](Environments/BaseHouse/BreakableWall/brick-wall-prompt.md) · [Blender source](Environments/BaseHouse/BreakableWall/brick.blend) · [Unity FBX](../../Assets/Models/Modules/House/WallBrick.fbx)

The dust uses frames 04–07 of Chill Miner's `Assets/Art/Textures/Vfx/InkVfxAtlas.png` from revision `82d4620`. FireSlop packs those frames into a 512 × 128 strip for its brick dust particle prefab.

### Cat

![Cat modeling reference](Characters/Cat/modeling-sheet.png)

Oversized spherical eyes, rounded muzzle, compact four-legged body and thick tail, based on the Canvas cat reference. Body and rear views extend the face-only source. This is an art request; cat gameplay remains outside the current demo specification.

[Image](Characters/Cat/modeling-sheet.png) · [Generation prompt](Characters/Cat/prompt.json)

![Cat Blender model](Characters/Cat/model-preview.png)

[Blender source](Characters/Cat/source.blend) · [FBX](../../Assets/Models/Characters/Cat.fbx) · 2,362 triangles.

### Rescue Trampoline

![Rescue Trampoline modeling reference](Props/Trampoline/modeling-sheet.png)

Octagonal padded rim and taut center, adapted from the handheld Canvas reference to the stationary ground-supported trampoline in the GDD. Model frame, padding and fabric separately. Use four U-shaped supports arranged symmetrically under alternating sides; the drawn front and side support projections are approximate and must be reconciled in the 3D blockout.

[Image](Props/Trampoline/modeling-sheet.png) · [Generation prompt](Props/Trampoline/prompt.json)

![Trampoline Blender model](Props/Trampoline/model-preview.png)

[Blender source](Props/Trampoline/source.blend) · [FBX](../../Assets/Models/Props/Trampoline.fbx) · 844 triangles.

## Review notes

All eleven images were visually inspected for subject, view coverage and readable construction. Furniture cushion counts and four drone rotors are consistent. Minor perspective differences remain in drawn turnarounds; use orthographic views as construction guidance. The eleven Blender models and FBX exports now exist; see [model production status](ModelProduction/README.md). Blender previews are not Unity captures. Unity imported all 36 FBX assets with shared URP materials. Connected module prefabs and an Edit Mode review scene are saved; [Unity previews](ModelProduction/README.md#unity-previews) were visually inspected. No player build or gameplay validation was performed.

The reference canvas keeps its original nodes. A separate draft group presents these sheets through a vault symlink to the single maintained art directory.
