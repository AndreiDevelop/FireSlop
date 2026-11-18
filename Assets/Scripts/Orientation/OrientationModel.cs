using System;

namespace FireSlop.Orientation
{
    public sealed class OrientationModel
    {
        public bool IsBlocking { get; private set; }
        public event Action Changed;

        public void SetBlocking(bool isBlocking)
        {
            if (IsBlocking == isBlocking)
            {
                return;
            }

            IsBlocking = isBlocking;
            Changed?.Invoke();
        }
    }
}
