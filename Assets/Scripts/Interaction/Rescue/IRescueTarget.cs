using UnityEngine;

namespace FireSlop.Interaction
{
    public interface IRescueTarget
    {
        Vector3 DeliveryPosition { get; }
        bool TryRescue(Vector3 deliveryPosition);
    }
}
