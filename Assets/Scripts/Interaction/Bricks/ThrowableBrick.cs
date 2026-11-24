using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class ThrowableBrick : MonoBehaviour, IContextActionTarget
    {
        [SerializeField] private string _contentId;
        [SerializeField] private Collider _pickupCollider;
        [SerializeField] private Rigidbody _body;
        [SerializeField] private ThrowableBrickConfigSO _config;
        [SerializeField] private InteractionConfigSO _interactionConfig;

        private BrickThrowController _inventory;
        private CarryReleaseShape _shape;
        private Vector3 _authoredPosition;
        private Quaternion _authoredRotation;

        public string ContentId => _contentId;

        [Inject]
        public void Construct(BrickThrowController inventory)
        {
            _inventory = inventory;
        }

        private void Awake()
        {
            _body.mass = _config.Mass;
            _authoredPosition = transform.position;
            _authoredRotation = transform.rotation;
            _shape = CarryReleaseShape.Capture(_pickupCollider, transform);
        }

        public ContextActionPrompt GetPrompt(Collider hit)
        {
            return hit == _pickupCollider && _inventory.CanCollect
                ? new ContextActionPrompt(HudActionId.PickUp)
                : default;
        }

        public void Interact()
        {
            if (isActiveAndEnabled)
            {
                _inventory.Collect(this);
            }
        }

        internal bool CanLaunch(ThrowPoseView pose)
        {
            return _shape.IsPathClear(pose.EyePosition, pose.LaunchPosition, pose.HoldPoint.rotation,
                _interactionConfig.AimCollisionMask);
        }

        internal void Launch(ThrowPoseView pose)
        {
            transform.SetPositionAndRotation(pose.LaunchPosition, pose.HoldPoint.rotation);
            gameObject.SetActive(true);
            _body.position = transform.position;
            _body.rotation = transform.rotation;
            _body.linearVelocity = pose.LaunchVelocity;
            _body.angularVelocity = Vector3.zero;
        }

        private void FixedUpdate()
        {
            if (_body.position.y < _authoredPosition.y - _config.RecoveryDepth)
            {
                _body.position = _authoredPosition;
                _body.rotation = _authoredRotation;
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.sqrMagnitude < _config.BreakSpeed * _config.BreakSpeed)
            {
                return;
            }

            var section = collision.collider.GetComponentInParent<WindowSectionView>();
            if (section != null)
            {
                section.ApplyImpact(collision.collider, -collision.GetContact(0).normal, _interactionConfig);
            }
        }
    }
}
