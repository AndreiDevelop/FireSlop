using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class BrickDebrisView : MonoBehaviour
    {
        [SerializeField] private BreakableFragment[] _pieces;

        internal void Scatter(Vector3 direction, Material material, InteractionConfigSO config)
        {
            foreach (var piece in _pieces)
            {
                piece.GetComponent<MeshRenderer>().sharedMaterial = material;
                piece.Detach(transform, direction, config);
            }
        }
    }
}
