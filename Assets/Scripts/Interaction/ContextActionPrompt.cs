namespace FireSlop.Interaction
{
    public readonly struct ContextActionPrompt
    {
        public ContextActionPrompt(HudActionId actionId, HudActionId blockedActionId = HudActionId.None)
        {
            ActionId = actionId;
            BlockedActionId = blockedActionId;
        }

        public HudActionId ActionId { get; }
        public HudActionId BlockedActionId { get; }
        public bool IsAvailable => ActionId != HudActionId.None;
        public bool IsBlocked => BlockedActionId != HudActionId.None;
    }
}
