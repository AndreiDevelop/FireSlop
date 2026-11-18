using UnityEngine;

namespace FireSlop.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Openable))]
    public sealed class CabinetContextTarget : MonoBehaviour, IContextActionTarget
    {
        private Openable _openable;

        private void Awake()
        {
            _openable = GetComponent<Openable>();
        }

        public ContextActionPrompt GetPrompt(Collider hit)
        {
            if (!_openable.CanToggle)
            {
                return default;
            }

            var actionId = _openable.IsOpen ? HudActionId.CloseCabinet : HudActionId.OpenCabinet;
            return new ContextActionPrompt(actionId);
        }

        public void Interact()
        {
            _openable.TryToggle();
        }
    }
}
