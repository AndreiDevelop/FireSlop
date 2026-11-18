---
name: fireslop-interactions
description: Implement or review object interactions in this project when adding a receiver, sharing behavior, changing input priority, or connecting action feedback to the HUD.
---

Read the project AGENTS.md and its engineering guide. Apply [capability contracts and dependency ownership](../../../Documentation/Engineering/CODE_STYLE_AND_ARCHITECTURE.md#capability-contracts-and-dependency-ownership).

1. Trace a comparable action from input to state change and feedback. Identify the caller, receiver, state owner and dependency lifetime in the current code.
2. Locate the existing contract or component that covers the requested behavior. Inspect its callers, bindings and authored references before extending it.
3. Verify through the normal entry point. Choose a case that tests the boundary you changed: two instances for state isolation, an unavailable action for eligibility, or competing inputs for priority.
