using FireSlop.Firefighter;
using UnityEngine;
using Zenject;

namespace FireSlop.Drone
{
    public sealed class DroneController : MonoBehaviour
    {
        private const float Clearance = 0.025f;

        [SerializeField] private DroneConfigSO _config;
        [SerializeField] private BoxCollider _deck;
        [SerializeField] private BoxCollider _flightBounds;
        [SerializeField] private BoxCollider _landingBounds;
        [SerializeField] private string _contentId;

        private readonly RaycastHit[] _hits = new RaycastHit[32];
        private readonly Collider[] _overlaps = new Collider[32];
        private DroneModel _model;
        private MissionInputModel _input;
        private Vector3 _origin;

        public string ContentId => _contentId;

        [Inject]
        public void Construct(DroneModel model, MissionInputModel input)
        {
            _model = model;
            _input = input;
            _origin = transform.position;
        }

        public bool IsStandingOnDeck(CharacterController pilot)
        {
            var feet = pilot.bounds.center;
            feet.y = pilot.bounds.min.y;
            var localFeet = _deck.transform.InverseTransformPoint(feet) - _deck.center;
            var half = _deck.size * 0.5f;
            var tolerance = pilot.skinWidth + Clearance;
            var topTolerance = pilot.isGrounded ? tolerance + pilot.skinWidth : tolerance;
            return Mathf.Abs(localFeet.x) <= half.x &&
                   Mathf.Abs(localFeet.z) <= half.z &&
                   localFeet.y >= half.y - tolerance && localFeet.y <= half.y + topTolerance;
        }

        public bool IsDockedAtWindow(Collider opening)
        {
            var bounds = opening.bounds;
            var sill = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            var closest = _deck.ClosestPoint(sill);
            return (closest - sill).sqrMagnitude <= _config.WindowDockTolerance * _config.WindowDockTolerance;
        }

        public void TogglePilot()
        {
            _model.TogglePilot(Time.time, _config.HoverAfterExitSeconds);
            _input.Cancel();
        }

        public void Fly(CharacterController pilot, Vector2 move, float vertical, float yaw, float deltaTime)
        {
            var rotation = Quaternion.Euler(0f, yaw, 0f);
            var direction = rotation * new Vector3(move.x, 0f, move.y);
            var delta = direction * (_config.HorizontalSpeed * deltaTime);
            delta.y = vertical * _config.VerticalSpeed * deltaTime;
            var target = transform.position + delta;
            var horizontal = Vector3.ProjectOnPlane(target - _origin, Vector3.up);
            horizontal = Vector3.ClampMagnitude(horizontal, _config.FlightRadius);
            target.x = _origin.x + horizontal.x;
            target.z = _origin.z + horizontal.z;
            target.y = Mathf.Clamp(target.y, _origin.y - _config.FlightHeight, _origin.y + _config.FlightHeight);

            var previous = transform.position;
            var pilotOffset = transform.InverseTransformPoint(pilot.transform.position);
            if (CanTurn(_flightBounds, rotation, pilot) && CanTurn(_landingBounds, rotation, pilot))
            {
                transform.rotation = rotation;
            }

            Move(target - previous, pilot);
            var wasEnabled = pilot.enabled;
            pilot.enabled = false;
            pilot.transform.position = transform.TransformPoint(pilotOffset);
            pilot.enabled = wasEnabled;
        }

        public Vector3 GetCameraPosition(Transform eyes, CharacterController pilot)
        {
            var focus = eyes.position + Vector3.up * _config.CameraHeight;
            var offset = -eyes.forward * _config.CameraDistance;
            var distance = GetClearDistance(focus, Vector3.one * Clearance, offset, pilot, Quaternion.identity);
            return focus + offset.normalized * distance;
        }

        private void LateUpdate()
        {
            if (_input != null && _input.IsEnabled && _model.CanDescend(Time.time) &&
                transform.position.y >= _origin.y - _config.FlightHeight)
            {
                Move(Vector3.down * (_config.DescentSpeed * Time.deltaTime), null);
            }
        }

        private void Move(Vector3 delta, CharacterController pilot)
        {
            var horizontal = Vector3.ProjectOnPlane(delta, Vector3.up);
            transform.position += horizontal.normalized * GetFlightDistance(horizontal, pilot);

            var vertical = Vector3.up * delta.y;
            transform.position += vertical.normalized * GetFlightDistance(vertical, pilot);
        }

        private float GetFlightDistance(Vector3 delta, CharacterController pilot)
        {
            var upper = GetClearDistance(transform.TransformPoint(_flightBounds.center), GetHalfSize(_flightBounds),
                delta, pilot, transform.rotation);
            var lower = GetClearDistance(transform.TransformPoint(_landingBounds.center), GetHalfSize(_landingBounds),
                delta, pilot, transform.rotation);
            return Mathf.Min(upper, lower);
        }

        private bool CanTurn(BoxCollider volume, Quaternion rotation, CharacterController pilot)
        {
            if (transform.rotation == rotation)
            {
                return true;
            }

            var center = transform.position + rotation * Vector3.Scale(volume.center, volume.transform.lossyScale);
            var count = Physics.OverlapBoxNonAlloc(center, GetHalfSize(volume), _overlaps, rotation,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (var index = 0; index < count; index++)
            {
                if (IsObstacle(_overlaps[index], pilot))
                {
                    return false;
                }
            }

            return count < _overlaps.Length;
        }

        private static Vector3 GetHalfSize(BoxCollider volume)
        {
            var size = Vector3.Scale(volume.size, volume.transform.lossyScale);
            return Vector3.Max(size * 0.5f - Vector3.one * Clearance, Vector3.one * Clearance);
        }

        private bool IsObstacle(Collider collider, CharacterController pilot)
        {
            return !collider.transform.IsChildOf(transform) &&
                   (pilot == null || !collider.transform.IsChildOf(pilot.transform));
        }

        private float GetClearDistance(Vector3 center, Vector3 half, Vector3 delta, CharacterController pilot,
            Quaternion rotation)
        {
            var distance = delta.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return 0f;
            }

            var count = Physics.BoxCastNonAlloc(center, half, delta / distance, _hits, rotation,
                distance + Clearance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (var index = 0; index < count; index++)
            {
                if (IsObstacle(_hits[index].collider, pilot))
                {
                    distance = Mathf.Min(distance, Mathf.Max(0f, _hits[index].distance - Clearance));
                }
            }

            return distance;
        }
    }
}
