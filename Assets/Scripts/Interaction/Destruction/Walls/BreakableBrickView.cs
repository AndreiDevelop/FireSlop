using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class BreakableBrickView : MonoBehaviour
    {
        [SerializeField] private BoxCollider _solid;
        [SerializeField] private MeshRenderer _surface;
        [SerializeField] private BrickDebrisView _debris;

        internal BreakableWallView Wall { get; private set; }
        internal bool IsSolid => _solid.enabled && gameObject.activeInHierarchy;
        internal float Top => _solid.bounds.max.y;

        private void Awake()
        {
            Wall = GetComponentInParent<BreakableWallView>();
        }

        internal bool TryBreak(Vector3 direction, InteractionConfigSO config)
        {
            if (!_solid.enabled)
            {
                return false;
            }

            _solid.enabled = false;
            var debris = Instantiate(_debris, transform.position, transform.rotation, transform.parent);
            debris.Scatter(direction, _surface.sharedMaterial, config);
            if (Wall != null)
            {
                Wall.PlayBrickBreak();
            }

            gameObject.SetActive(false);
            return true;
        }
    }
}
