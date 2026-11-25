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
        [SerializeField] private Bounds _droneLandingArea;

        private RescueController _carrier;
        private InteractionModel _interaction;
        private MissionModel _mission;
        private DroneController _drone;
        private CharacterController _firefighter;

        public string ContentId => _contentId;

        [Inject]
        public void Construct(MissionModel mission, InteractionModel interaction, RescueController carrier,
            Camera camera, [InjectOptional] DroneController drone)
        {
            _mission = mission;
            _drone = drone;
            _firefighter = camera.GetComponentInParent<CharacterController>();
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
                var landingHeight = transform.TransformPoint(_droneLandingArea.center).y;
                deliveryPoint.y = Mathf.Clamp(landingHeight, _drone.LandingHeight, deliveryPoint.y);
                if (_droneLandingArea.Contains(transform.InverseTransformPoint(deliveryPoint)))
                {
                    target.TryRescue(target.DeliveryPosition);
                    return;
                }
            }

            else if (_firefighter != null)
            {
                var feet = _firefighter.bounds.center;
                feet.y = _firefighter.bounds.min.y + _firefighter.skinWidth;
                if (_volume.bounds.Contains(feet))
                {
                    target.TryRescue(target.DeliveryPosition);
                    return;
                }
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
