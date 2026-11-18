using UnityEngine;

namespace FireSlop.Interaction
{
    public interface IContextActionTarget
    {
        ContextActionPrompt GetPrompt(Collider hit);
        void Interact();
    }
}
