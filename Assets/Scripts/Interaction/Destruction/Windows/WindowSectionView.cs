using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class WindowSectionView : MonoBehaviour, IHitTarget
    {
        [SerializeField] private BreakableWindowView _window;
        [SerializeField] private Renderer _intactSurface;
        [SerializeField] private Collider _solid;
        [SerializeField] private BreakableFragment[] _fragments;

        public void ApplyHit(Ray ray, RaycastHit hit, InteractionConfigSO config)
        {
            ApplyImpact(hit.collider, ray.direction, config);
        }

        public void ApplyImpact(Collider collider, Vector3 direction, InteractionConfigSO config)
        {
            if (collider == _solid && _solid.enabled)
            {
                _window.Hit(this, direction, config);
            }
        }

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
