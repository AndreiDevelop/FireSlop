using FireSlop.Mission;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class ResidentView : Carryable
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private ResidentCarryRagdoll _carryRagdoll;
        [SerializeField] private InteractionConfigSO _config;
        [SerializeField] private Vector3 _standingPosition;
        [SerializeField] private Quaternion _standingRotation = Quaternion.identity;

        private InteractionModel _model;
        private MissionModel _mission;
        private bool _hasBeenRevealed;

        public bool IsRescued => _mission != null && _mission.IsWon;

        public bool CanBeKnockedDown => _hasBeenRevealed && PickupCollider.enabled && !IsRescued;

        [Inject]
        public void Construct(InteractionModel model, MissionModel mission)
        {
            _model = model;
            _mission = mission;
            _mission.Won += Rescue;
            Body.mass = _config.ResidentMass;
            _carryRagdoll.Initialize();
            _model.CabinetRevealed += Reveal;
        }

        internal void PlaceInCabinet(Transform cabinet)
        {
            transform.SetPositionAndRotation(cabinet.position, cabinet.rotation);
            Body.position = cabinet.position;
            Body.rotation = cabinet.rotation;
        }

        protected override void OnAttached(Transform shoulder)
        {
            _carryRagdoll.HangFrom(shoulder);
            _visual.gameObject.SetActive(false);
        }

        protected override void OnReleasing()
        {
            _carryRagdoll.Stow();
            _visual.gameObject.SetActive(true);
        }

        private void Rescue()
        {
            Body.isKinematic = true;
        }

        private void Reveal()
        {
            _hasBeenRevealed = true;
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
