using UnityEngine;

namespace FireSlop.Interaction
{
    public readonly struct ContextActionPrompt
    {
        public ContextActionPrompt(string label, Sprite icon = null, string blockedMessage = null)
        {
            Label = label;
            Icon = icon;
            BlockedMessage = blockedMessage;
        }

        public string Label { get; }
        public Sprite Icon { get; }
        public string BlockedMessage { get; }
        public bool IsAvailable => !string.IsNullOrEmpty(Label);
    }
}
