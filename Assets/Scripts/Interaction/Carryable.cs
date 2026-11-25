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

        [SerializeField] private bool _useRescueGrip;
        [SerializeField] private Vector3 _carryOffset;
        [SerializeField] private Vector3 _carryEuler;

        internal bool UsesRescueGrip => _useRescueGrip;
        private RescueController _carrier;
        private Transform _worldParent;
        private Vector3 _originalLocalScale;
        private CarryReleaseShape _releaseShape;
        private RigidbodyInterpolation _worldInterpolation;
        private bool _isCarried;

        public string ContentId => _contentId;
        public Collider PickupCollider => _pickupCollider;
        public bool IsFree => !_body.isKinematic;
        public bool IsCarried => _isCarried;
        protected Rigidbody Body => _body;

        internal virtual bool CanPickUp => isActiveAndEnabled && _body != null && _pickupCollider != null && _pickupCollider.enabled &&
            !string.IsNullOrWhiteSpace(_contentId);

        [Inject]
        internal void BindCarrier([InjectOptional] RescueController carrier)
        {
            _carrier = carrier;
        }

        public virtual ContextActionPrompt GetPrompt(Collider hit)
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
            _isCarried = true;
            _worldParent = transform.parent;
            _originalLocalScale = transform.localScale;
            _releaseShape = CarryReleaseShape.Capture(_pickupCollider, transform);
            _pickupCollider.enabled = false;
            _worldInterpolation = _body.interpolation;
            _body.interpolation = RigidbodyInterpolation.None;
            _body.isKinematic = true;
            transform.SetParent(shoulder, false);
            transform.SetLocalPositionAndRotation(_carryOffset, Quaternion.Euler(_carryEuler));
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
            _body.interpolation = _worldInterpolation;
            _pickupCollider.enabled = true;
            _body.isKinematic = false;
            _body.linearVelocity = velocity;
            _body.angularVelocity = Vector3.zero;
            _isCarried = false;
            return true;
        }

        internal void Deliver(Vector3 position)
        {
            if (_isCarried)
            {
                OnReleasing();
                transform.SetParent(_worldParent, false);
                transform.localScale = _originalLocalScale;
            }

            transform.position = position;
            _body.position = position;
            if (_isCarried)
            {
                _body.interpolation = _worldInterpolation;
            }
            _body.isKinematic = false;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            _pickupCollider.enabled = true;
            _isCarried = false;
        }

        protected virtual void OnAttached(Transform shoulder)
        {
        }

        protected virtual void OnReleasing()
        {
        }
    }
}
