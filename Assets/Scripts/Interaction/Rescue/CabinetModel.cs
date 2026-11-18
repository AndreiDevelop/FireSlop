using System;

namespace FireSlop.Interaction
{
    internal sealed class CabinetModel
    {
        public bool IsOpen { get; private set; }
        public event Action Changed;

        public void Toggle()
        {
            IsOpen = !IsOpen;
            Changed?.Invoke();
        }
    }
}
