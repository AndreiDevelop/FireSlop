using System;
using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class BreakableWallView : MonoBehaviour
    {
        [SerializeField] private string _contentId;
        [SerializeField] private Collider _solid;
        [SerializeField] private Renderer _wall;
        [SerializeField] private BreakableFragment[] _fragments;
        [SerializeField] private Mesh[] _dentedMeshes;

        private BreakableWallModel _state;

        public string ContentId => _contentId;
        public bool IsBroken => _state != null && _state.IsBroken;
        public int DamageHits => _state?.DamageHits ?? 0;
        public int DetachedFragments => _state?.DetachedFragments ?? 0;

        private void Awake()
        {
            _state = new BreakableWallModel(_fragments.Length);
        }

        internal void HitIntact(Ray ray, InteractionConfigSO config)
        {
            if (DamageHits != 0)
            {
                return;
            }

            var struck = BreakableFragment.FindFirstHit(_solid, _fragments, ray, config.HammerReach);
            if (struck != null)
            {
                Hit(struck, ray.direction, config);
            }
        }

        internal void Hit(BreakableFragment struck, Vector3 direction, InteractionConfigSO config)
        {
            var index = Array.IndexOf(_fragments, struck);
            if (index < 0 || !_state.TryHit(index, config.WallFragmentsToCollapse, out var damage))
            {
                return;
            }

            if (DamageHits == 1)
            {
                _wall.enabled = false;
                foreach (var fragment in _fragments)
                {
                    fragment.Reveal();
                }
            }

            if (damage == WallFragmentDamage.Dented)
            {
                struck.GetComponent<MeshFilter>().sharedMesh = _dentedMeshes[index];
                return;
            }

            struck.Detach(transform, direction, config);
            if (!IsBroken)
            {
                return;
            }

            foreach (var fragment in _fragments)
            {
                fragment.Detach(transform, direction, config);
            }
        }
    }
}
