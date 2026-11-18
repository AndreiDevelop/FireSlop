using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace FireSlop.Interaction
{
    [DisallowMultipleComponent]
    public class Carryable : MonoBehaviour, IContextActionTarget
    {
        [SerializeField] private string _contentId;
        [FormerlySerializedAs("_capsule")]
        [SerializeField] private Collider _pickupCollider;
        [SerializeField] private Rigidbody _body;

        private RescueController _carrier;
        private Transform _worldParent;
        private Vector3 _originalLocalScale;
        private CarryReleaseShape _releaseShape;

        public string ContentId => _contentId;
        public Collider PickupCollider => _pickupCollider;
        public bool IsFree => !_body.isKinematic;
        protected Rigidbody Body => _body;

        internal bool CanPickUp => isActiveAndEnabled && _body != null && _pickupCollider != null && _pickupCollider.enabled &&
            !string.IsNullOrWhiteSpace(_contentId);

        [Inject]
        internal void BindCarrier([InjectOptional] RescueController carrier)
        {
            _carrier = carrier;
        }

        public ContextActionPrompt GetPrompt(Collider hit)
        {
            if (hit != _pickupCollider || _carrier == null || !_carrier.CanPickUp(this))
            {
                return default;
            }

            return new ContextActionPrompt(HudActionId.PickUp);
        }

        public void Interact()
        {
            _carrier.TryPickUp(this);
        }

        internal void Attach(Transform shoulder)
        {
            _worldParent = transform.parent;
            _originalLocalScale = transform.localScale;
            _releaseShape = CarryReleaseShape.Capture(_pickupCollider, transform);
            _pickupCollider.enabled = false;
            _body.isKinematic = true;
            transform.SetParent(shoulder, false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            OnAttached(shoulder);
        }

        internal bool TryRelease(Vector3 eyePosition, Vector3 forward, float distance, LayerMask mask, Vector3 velocity)
        {
            if (!_releaseShape.TryPlace(eyePosition, forward, distance, mask, out var position))
            {
                return false;
            }

            OnReleasing();
            var rotation = Quaternion.LookRotation(forward);
            transform.SetParent(_worldParent, false);
            transform.localScale = _originalLocalScale;
            transform.SetPositionAndRotation(position, rotation);
            _body.position = position;
            _body.rotation = rotation;
            _pickupCollider.enabled = true;
            _body.isKinematic = false;
            _body.linearVelocity = velocity;
            _body.angularVelocity = Vector3.zero;
            return true;
        }

        protected virtual void OnAttached(Transform shoulder)
        {
        }

        protected virtual void OnReleasing()
        {
        }
    }
}
