using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class BreakableBrickView : MonoBehaviour, IHitTarget
    {
        [SerializeField] private BoxCollider _solid;
        [SerializeField] private MeshRenderer _surface;
        [SerializeField] private BrickDebrisView _debris;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Mesh _crackedMesh;

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
            if (hit.collider != _solid || !IsSolid)
            {
                return;
            }

            _dust.Play(hit, config.BrickDust);
            if (Wall != null)
            {
                Wall.ApplyBrickHit(this, ray.direction, config);
                return;
            }

            ReleaseDebris(ray.direction, config);
        }

        internal void ShowCrack(Material interior)
        {
            if (_meshFilter == null || _crackedMesh == null)
            {
                Debug.LogError("Assign a mesh filter and cracked mesh to the breakable brick.", this);
                return;
            }

            _meshFilter.sharedMesh = _crackedMesh;
            _surface.sharedMaterials = new[] { _surface.sharedMaterial, interior };
        }

        internal void DisableCollision()
        {
            _solid.enabled = false;
        }

        internal void ReleaseDebris(Vector3 direction, InteractionConfigSO config)
        {
            DisableCollision();
            var debris = Instantiate(_debris, transform.position, transform.rotation, transform.parent);
            debris.Scatter(direction, _surface.sharedMaterial, config);
            gameObject.SetActive(false);
        }
    }
}
