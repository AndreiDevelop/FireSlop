using System;
using FireSlop.Drone;
using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.Mission
{
    public sealed class RescueZoneView : MonoBehaviour
    {
        [SerializeField] private string _contentId;
        [SerializeField] private BoxCollider _volume;

        private RescueController _carrier;
        private InteractionModel _interaction;
        private MissionModel _mission;
        private DroneController _drone;

        public string ContentId => _contentId;

        [Inject]
        public void Construct(MissionModel mission, InteractionModel interaction, RescueController carrier,
            [InjectOptional] DroneController drone)
        {
            _mission = mission;
            _drone = drone;
            _interaction = interaction;
            _carrier = carrier;
            if (_volume == null || !_volume.isTrigger || string.IsNullOrWhiteSpace(_contentId))
            {
                throw new InvalidOperationException("Rescue zone requires an ID and trigger volume.");
            }
        }

        private void FixedUpdate()
        {
            if (_mission == null || !_mission.IsActive || !(_carrier.Carried is IRescueTarget target))
            {
                return;
            }

            var deliveryPoint = target.DeliveryPosition;
            if (_interaction.IsPilotingDrone && _drone != null)
            {
                // Project the carried target onto the skid height to detect delivery at deck touchdown.
                deliveryPoint.y = Mathf.Clamp(_volume.bounds.center.y, _drone.LandingHeight, deliveryPoint.y);
            }

            var local = _volume.transform.InverseTransformPoint(deliveryPoint) - _volume.center;
            var half = _volume.size * 0.5f;
            if (Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z)
            {
                target.TryRescue(target.DeliveryPosition);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Accept(other);
        }

        private void OnTriggerStay(Collider other)
        {
            Accept(other);
        }

        private void Accept(Collider other)
        {
            if (_mission != null && _mission.IsActive && other.TryGetComponent<IRescueTarget>(out var target))
            {
                target.TryRescue(target.DeliveryPosition);
            }
        }
    }
}
