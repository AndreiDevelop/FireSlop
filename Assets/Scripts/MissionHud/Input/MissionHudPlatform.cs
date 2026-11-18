using System;
using FireSlop.Orientation;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class MissionHudPlatform : ITickable
    {
        public bool UsesTouchControls { get; private set; } = MobileOrientationPlatform.IsMobile;
        public event Action Changed;

        public void Tick()
        {
#if UNITY_EDITOR
            var touch = MobileOrientationPlatform.IsMobile;
            if (UsesTouchControls != touch)
            {
                UsesTouchControls = touch;
                Changed?.Invoke();
            }
#endif
        }
    }
}
