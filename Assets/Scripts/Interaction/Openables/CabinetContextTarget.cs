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

            return new ContextActionPrompt(_openable.IsOpen ? "Close cabinet" : "Open cabinet");
        }

        public void Interact()
        {
            _openable.TryToggle();
        }
    }
}
