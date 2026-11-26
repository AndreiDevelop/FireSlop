# Shift Log UI Concepts

Draft for review · 2026-11-25 · Owner TBD

## Unity implementation

The six screens are authored in the dispatch, settings and mission-result prefabs. The menu uses the original locker room and the animated wardrobe model wearing the saved outfit. The victory report uses the current resident portrait and reads the count and time from the mission model.

The [Play Mode contact sheet](PlayMode/overview.jpg) shows the six screens at 1920 × 1080. On 2026-11-25 the menu-to-mission-to-menu path was also exercised at 640 × 360 and 800 × 600. The checks covered idle progression, calendar credit, ranking, outfit selection by category, audio levels and the victory report after three scripted rescues. Visible TMP labels reported no overflow in these runs.

## Current low-poly screen designs

[Six-screen preview](Screens/overview.jpg) · [Dispatch](Screens/main.png) · [Animated dispatch preview](Screens/main-idle.webp) · [Daily rewards](Screens/daily-rewards.png) · [Local ranking](Screens/ranking.png) · [Wardrobe](Screens/wardrobe.png) · [Settings](Screens/settings.png) · [Victory](Screens/victory.png)

The dispatch preview samples a full 9.93-second Idle cycle from the FirefighterWardrobe Animator in the authored DispatchMenu prefab. The victory portrait is a Unity camera capture of RescueResident.

These 1280 × 720 mockups use the LockerRoom background referenced by DispatchMenu, the original HoseFlightLoading illustration, project character renders, wardrobe sprites, ranking avatar assignments, hose icon and wallet coin. The new gift, podium and speaker symbols use flat polygon faces. Amounts, selections and time illustrate one possible screen state.

## Earlier composition studies

The dispatch ticket starts the mission. The incident report closes it. Warm paper and petrol-blue ink connect the two, with vermilion reserved for the main action or selected control. In the field, keep texture on scenery and leave room around the aim point. Read each sheet left to right, top to bottom.

| Concept sheet | Upper row | Lower row |
| --- | --- | --- |
| [Station](station.png) | Hose loading gauge · Dispatch desk | Seven-day rewards · Local ranking |
| [Equipment and results](equipment.png) | Outfit purchase · Owned eyewear fitting | Audio settings · Rescue result |
| [Mission](mission.png) | Desktop ground controls · Touch ground controls | Touch drone piloting · Cat removal |
| [Context actions](context.png) | Carry to rescue tent · Drone landing preview | Brick trajectory · Planned three-quest overlay |

Use the wardrobe category icons as one selector. Frame outfits at full height and eyewear at face level. Keep collection, availability and purchase states distinguishable by check, lock and price. Keep the wallet read-only. The ranking uses the project's compact number formatter.

During missions, anchor rescue progress and time consistently across states. Show the bound desktop action or its touch equivalent. Keep tool slots in their authored order and use the existing cat-removal input. Keycaps, currency drawings, scene art and sample balances in these illustrations are illustrative. Production uses the authored resource glyph, current bindings and live data. The three-quest overlay depicts the planned expansion. These static compositions have not been tested in Unity or on a phone.

[Art direction](../../ART_DIRECTION_GUIDELINES.md) · [Dispatch specification](../../../../FireSlop/Documentation/GDD/Design/dispatch-and-progression.md) · [Generation prompts](prompts.json)
