using FireSlop.Mission;
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
        [SerializeField] private ResidentCarryRagdoll _carryRagdoll;
        [SerializeField] private InteractionConfigSO _config;
        [SerializeField] private Vector3 _standingPosition;
        [SerializeField] private Quaternion _standingRotation = Quaternion.identity;

        private InteractionModel _model;
        private MissionModel _mission;

        public bool IsRescued => _mission != null && _mission.IsWon;

        public string ContentId => _contentId;
        public CapsuleCollider Capsule => _capsule;

        [Inject]
        public void Construct(InteractionModel model, MissionModel mission)
        {
            _model = model;
            _mission = mission;
            _mission.Won += Rescue;
            _body.mass = _config.ResidentMass;
            _carryRagdoll.Initialize();
            _model.CabinetRevealed += Reveal;
        }

        public void Attach(Transform shoulder)
        {
            _capsule.enabled = false;
            _body.isKinematic = true;
            transform.SetParent(shoulder, false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _carryRagdoll.HangFrom(shoulder);
            _visual.gameObject.SetActive(false);
        }

        public void Release(Transform worldParent, Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            _carryRagdoll.Stow();
            _visual.gameObject.SetActive(true);
            transform.SetParent(worldParent, true);
            transform.SetPositionAndRotation(position, rotation);
            _body.position = position;
            _body.rotation = rotation;
            _capsule.enabled = true;
            _body.isKinematic = false;
            _body.linearVelocity = velocity;
            _body.angularVelocity = Vector3.zero;
        }

        private void Rescue()
        {
            _body.isKinematic = true;
        }

        private void Reveal()
        {
            _visual.SetLocalPositionAndRotation(_standingPosition, _standingRotation);
        }

        private void OnDestroy()
        {
            if (_mission != null)
            {
                _mission.Won -= Rescue;
            }

            if (_carryRagdoll != null && _carryRagdoll.transform.parent == null)
            {
                Destroy(_carryRagdoll.gameObject);
            }

            if (_model != null)
            {
                _model.CabinetRevealed -= Reveal;
            }
        }
    }
}
