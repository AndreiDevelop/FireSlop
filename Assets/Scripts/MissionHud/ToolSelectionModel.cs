using System;

namespace FireSlop.MissionHud
{
    public sealed class ToolSelectionModel
    {
        public int Selected { get; private set; }
        public event Action Changed;

        public void Select(int slot)
        {
            if (slot == Selected || slot < 0 || slot > 1)
            {
                return;
            }

            Selected = slot;
            Changed?.Invoke();
        }
    }
}
