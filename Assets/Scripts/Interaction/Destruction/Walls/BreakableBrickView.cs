using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class BreakableBrickView : MonoBehaviour, IHitTarget
    {
        [SerializeField] private BoxCollider _solid;
        [SerializeField] private MeshRenderer _surface;
        [SerializeField] private BrickDebrisView _debris;

        private BrickDustEffect _dust;

        internal BreakableWallView Wall { get; private set; }
        internal bool IsSolid => _solid.enabled && gameObject.activeInHierarchy;
        internal float Top => _solid.bounds.max.y;

        [Inject]
        internal void Construct(BrickDustEffect dust)
        {
            _dust = dust;
        }

        private void Awake()
        {
            Wall = GetComponentInParent<BreakableWallView>();
        }

        public void ApplyHit(Ray ray, RaycastHit hit, InteractionConfigSO config)
        {
            if (hit.collider != _solid || !_solid.enabled)
            {
                return;
            }

            _solid.enabled = false;
            var debris = Instantiate(_debris, transform.position, transform.rotation, transform.parent);
            debris.Scatter(ray.direction, _surface.sharedMaterial, config);
            if (Wall != null)
            {
                Wall.PlayBrickBreak();
            }

            _dust.Play(hit, config.BrickDust);
            gameObject.SetActive(false);
        }
    }
}
