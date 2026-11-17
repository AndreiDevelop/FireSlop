using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class BreakableBrickView : MonoBehaviour
    {
        [SerializeField] private BoxCollider _solid;
        [SerializeField] private MeshRenderer _surface;
        [SerializeField] private BrickDebrisView _debris;

        internal bool TryBreak(Vector3 direction, InteractionConfigSO config)
        {
            if (!_solid.enabled)
            {
                return false;
            }

            _solid.enabled = false;
            var debris = Instantiate(_debris, transform.position, transform.rotation, transform.parent);
            debris.Scatter(direction, _surface.sharedMaterial, config);
            gameObject.SetActive(false);
            return true;
        }
    }
}
