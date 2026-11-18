using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class WindowSectionView : MonoBehaviour
    {
        [SerializeField] private BreakableWindowView _window;
        [SerializeField] private Renderer _intactSurface;
        [SerializeField] private Collider _solid;
        [SerializeField] private BreakableFragment[] _fragments;

        public BreakableWindowView Window => _window;

        internal void ReleaseFragments(Transform owner, Vector3 direction, InteractionConfigSO config)
        {
            _solid.enabled = false;
            _intactSurface.enabled = false;

            foreach (var fragment in _fragments)
            {
                fragment.Detach(owner, direction, config);
            }
        }
    }
}
