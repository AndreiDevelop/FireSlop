using UnityEngine;
using UnityEngine.InputSystem;

namespace FireSlop.MissionHud
{
    internal static class MissionHudPlatform
    {
        public static bool UsesTouchControls => UnityEngine.Device.Application.isMobilePlatform ||
            (Application.platform == RuntimePlatform.WebGLPlayer && Touchscreen.current != null);
    }
}
