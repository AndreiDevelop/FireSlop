using System;

namespace FireSlop.Drone
{
    public sealed class DroneModel
    {
        private float _hoverUntil;

        public bool IsPiloted { get; private set; }
        public bool IsOnPlatform { get; private set; }

        public event Action ContextChanged;

        public void SetOnPlatform(bool isOnPlatform)
        {
            if (IsOnPlatform == isOnPlatform)
            {
                return;
            }

            IsOnPlatform = isOnPlatform;
            ContextChanged?.Invoke();
        }

        public void TogglePilot(float now, float hoverSeconds)
        {
            if (!IsPiloted && !IsOnPlatform)
            {
                return;
            }

            IsPiloted = !IsPiloted;
            if (!IsPiloted)
            {
                _hoverUntil = now + hoverSeconds;
            }

            ContextChanged?.Invoke();
        }

        public bool CanDescend(float now)
        {
            return !IsPiloted && !IsOnPlatform && now >= _hoverUntil;
        }
    }
}
