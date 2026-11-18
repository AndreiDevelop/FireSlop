using UnityEngine;

namespace FireSlop.Interaction
{
    public interface IHitTarget
    {
        void ApplyHit(Ray ray, RaycastHit hit, InteractionConfigSO config);
    }
}
