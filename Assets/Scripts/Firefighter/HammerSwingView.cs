using UnityEngine;

namespace FireSlop.Firefighter
{
    public sealed class HammerSwingView : MonoBehaviour
    {
        [SerializeField] private AnimationClip _swing;

        public float Duration => _swing != null ? _swing.length : 0f;

        public void Sample(float time)
        {
            if (_swing != null)
            {
                _swing.SampleAnimation(gameObject, time);
            }
        }
    }
}
