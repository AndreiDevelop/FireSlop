using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class BreakableWallView : MonoBehaviour
    {
        [SerializeField] private string _contentId;

        private BreakableBrickView[] _bricks;

        public string ContentId => _contentId;

        private void Awake()
        {
            _bricks = GetComponentsInChildren<BreakableBrickView>();
        }

        internal float GetNextSillRise(float feetY, float previousRise, float clearance)
        {
            var nextRise = float.PositiveInfinity;
            foreach (var brick in _bricks)
            {
                if (!brick.IsSolid)
                {
                    continue;
                }

                var rise = brick.Top - feetY + clearance;
                if (rise > previousRise)
                {
                    nextRise = Mathf.Min(nextRise, rise);
                }
            }

            return nextRise;
        }
    }
}
