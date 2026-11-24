# Dispatch menu verification

2026-11-24 · Unity 6000.6.0f1 · Editor and isolated preview scenes. [Feature specification](../../../FireSlop/Documentation/GDD/Design/dispatch-and-progression.md).

## Implemented scope

Dispatch menu with Play, Wardrobe, Rewards and Ranking, shared wallet, seven-day daily rewards adapted from Ducks Strike, cumulative local mission score, nine authored fictitious opponents, classic podium/list/pinned-player layout and compact numeric presentation throughout runtime UI. SO assets own reward amounts, mission score, rival names/scores/avatars. Existing profile keys, ownership and equipment are preserved.

## Data and controller checks

`AgentScripts/DispatchVerification.cs` passed **42 checks** through `unity command script run`. The original PlayerPrefs document and cursor state are restored in `finally`.

Covered old-profile compatibility, exact daily credit, duplicate claims, UTC midnight, skipped dates, clock rollback, reload, seven-day completion, invalid progress, integer overflow, preservation of ownership/equipment, independent currency and score, sorting and ties, first-place promotion and score persistence. Production `MissionController.Finish` was exercised with isolated dependencies: an unfinished mission cannot award, a completed mission adds the configured score and disables input, and duplicate completion cannot award again. Formatting checks covered suffix thresholds, fractional thousands, rounding promotion and integer extremes.

## Live Editor checks

A final Intro Play session advanced frames. Opening the calendar blocked raycasts to Play; seven cards were present, no text overflow was reported, and exactly one EventSystem was active. Navigation between wardrobe, daily rewards and ranking was exercised. Play loaded Main and HUD with an active three-resident mission. Returning through the scene-loading entry point reinitialized Intro with ten ranked entries and the unchanged score and daily claim count. The successful physical rescue route was not replayed in this task.

No new errors occurred in the final Play session after console cursor 21917. Earlier errors from transient wiring during concurrent Editor use remain in the retained console history; the final configuration and injection succeeded. Compilation completed without errors. No player or CI build was created.

## Compact number formatting

The shared `CompactNumber` utility now formats runtime balances, wardrobe prices/totals, ranking scores/ranks, daily amounts/feedback, quantities, progress and pagination. Clock durations retain their structured format. The current project rendering rule is recorded in the architecture guide.

## Visual checks

Isolated prefab scenes rendered at **640×360, 800×600 and 1920×1080**, including resizing an open panel. Reviewed ranking podiums, scroll viewport, pinned player, wallet, calendar and close controls. Repaired podium text bounds and wallet clearance; final ranking labels reported no overflow at all three sizes. Maximum balance/score values were tested on an in-memory profile, without saving them. The compact maximum score displays as `2.1B`.

The preview uses the saved DispatchMenu prefab, nested row/podium/card prefabs, actual fonts and authored sprites. Saved [ranking](../../Art/UI/Dispatch/ranking-640x360.png) and [calendar](../../Art/UI/Dispatch/calendar-800x600.png) images are preview renders, not browser/device evidence.

## Remaining checks and limits

Physical phone/browser input and a complete manual rescue-to-ranking run remain playtest work. Online leaderboards are explicitly deferred; rival entries are fictitious local configuration. Daily eligibility uses the offline device clock. Automatic calendar popups and currency flights from Ducks Strike were not ported. No end-user profile was left changed by verification. A later native-console check found two concurrent serialization errors: `ResidentView` and `CatCarryable` each duplicate the inherited `_carrier` field name. These belong to the ongoing carryable changes and were not edited by this menu task; they remain outside this verification result.

The Unity CLI sandbox hid the running Editor, so live CLI calls used approved access outside the sandbox. Consulted the installed `unity-cli` common-workflows and playmode-verification-loop references, the create-ui uGUI/scroll-view guides, and bundled uGUI layout, CanvasScaler and ScrollRect documentation.
