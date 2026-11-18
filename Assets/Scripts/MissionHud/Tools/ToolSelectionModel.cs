using System;

namespace FireSlop.MissionHud
{
    public sealed class ToolSelectionModel
    {
        private bool _enabled = true;

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        public int Selected { get; private set; }
        public event Action Changed;

        public void Select(int slot)
        {
            if (!_enabled || slot == Selected || slot < 0 || slot > 1)
            {
                return;
            }

            Selected = slot;
            Changed?.Invoke();
        }
    }
}
