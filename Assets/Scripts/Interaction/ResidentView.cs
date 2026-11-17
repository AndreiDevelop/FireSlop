using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class ResidentView : MonoBehaviour
    {
        [SerializeField] private string _contentId;
        [SerializeField] private CapsuleCollider _capsule;
        [SerializeField] private Rigidbody _body;
        [SerializeField] private Transform _visual;
        [SerializeField] private InteractionConfigSO _config;
        [SerializeField] private Vector3 _standingPosition;
        [SerializeField] private Quaternion _standingRotation = Quaternion.identity;

        private InteractionModel _model;

        public string ContentId => _contentId;
        public CapsuleCollider Capsule => _capsule;

        [Inject]
        public void Construct(InteractionModel model)
        {
            _model = model;
            _body.mass = _config.ResidentMass;
            _model.CabinetRevealed += Reveal;
        }

        public void Attach(Transform shoulder)
        {
            _capsule.enabled = false;
            _body.isKinematic = true;
            transform.SetParent(shoulder, false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public void Release(Transform worldParent, Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            transform.SetParent(worldParent, true);
            transform.SetPositionAndRotation(position, rotation);
            _body.position = position;
            _body.rotation = rotation;
            _capsule.enabled = true;
            _body.isKinematic = false;
            _body.linearVelocity = velocity;
            _body.angularVelocity = Vector3.zero;
        }

        private void Reveal()
        {
            _visual.SetLocalPositionAndRotation(_standingPosition, _standingRotation);
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.CabinetRevealed -= Reveal;
            }
        }
    }
}
