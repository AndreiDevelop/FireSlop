using UnityEngine;

namespace FireSlop.Interaction
{
    public interface IHitTarget
    {
        bool AcceptsTriggerHits => false;

        void ApplyHit(Ray ray, RaycastHit hit, InteractionConfigSO config);
    }
}
