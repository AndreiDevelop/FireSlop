#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine.Device;

namespace FireSlop.Orientation
{
    public static class MobileOrientationPlatform
    {
        public static bool IsMobile
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return OrientationGate_IsMobile() != 0;
#else
                return Application.isMobilePlatform;
#endif
            }
        }

        public static bool IsPortrait
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return OrientationGate_IsPortrait() != 0;
#else
                return Screen.height > Screen.width;
#endif
            }
        }

        public static int Subscribe(string receiver)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return OrientationGate_Subscribe(receiver);
#else
            return 0;
#endif
        }

        public static void Unsubscribe(int subscription)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OrientationGate_Unsubscribe(subscription);
#endif
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int OrientationGate_IsMobile();
        [DllImport("__Internal")]
        private static extern int OrientationGate_IsPortrait();
        [DllImport("__Internal")]
        private static extern int OrientationGate_Subscribe(string receiver);
        [DllImport("__Internal")]
        private static extern void OrientationGate_Unsubscribe(int subscription);
#endif
    }
}
