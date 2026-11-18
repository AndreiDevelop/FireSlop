using UnityEngine;

namespace FireSlop.Interaction
{
    public class WaterPushable : MonoBehaviour, IWaterTarget
    {
        public virtual void ApplyWater(in WaterImpact impact)
        {
            PushHitBody(impact);
        }

        internal static void PushHitBody(in WaterImpact impact)
        {
            var body = impact.Hit.rigidbody;
            if (impact.Impulse > 0f && body != null && !body.isKinematic)
            {
                body.AddForceAtPosition(impact.Direction * impact.Impulse, impact.Hit.point, ForceMode.Impulse);
            }
        }
    }
}
