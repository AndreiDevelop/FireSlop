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
        [SerializeField] private DroneRotorView _rotorView;
        [SerializeField] private string _contentId;

        [SerializeField] private AudioSource _rotorAudio;

        [SerializeField] private bool _requiresSignal;
        [SerializeField] private Vector3 _flightOrigin;

        private readonly RaycastHit[] _hits = new RaycastHit[32];
        private readonly Collider[] _overlaps = new Collider[32];
        private DroneModel _model;
        private MissionInputModel _input;
        private Vector3 _origin;
        private Vector3 _landingPoint;
        private ArrivalPhase _arrival;
        private enum ArrivalPhase
        {
            Available,
            Waiting,
            Climb,
            Approach,
            Descend
        }

        internal DroneConfigSO Config => _config;
        internal bool WaitingForSignal => _arrival == ArrivalPhase.Waiting;
        internal bool HasArrived => _arrival == ArrivalPhase.Available;
        internal float LandingHeight => _landingBounds.bounds.min.y;
        internal Vector3 Footprint => GetHalfSize(_flightBounds);
        private float CruiseY => _origin.y + _config.ApproachHeight;

        public string ContentId => _contentId;
        public float CameraTransitionSeconds => _config.CameraTransitionSeconds;

        [Inject]
        public void Construct(DroneModel model, MissionInputModel input)
        {
            _model = model;
            _input = input;
            _origin = _requiresSignal ? _flightOrigin : transform.position;
            _arrival = _requiresSignal ? ArrivalPhase.Waiting : ArrivalPhase.Available;
        }

        public bool IsStandingOnDeck(CharacterController pilot)
        {
            if (_arrival != ArrivalPhase.Available)
            {
                return false;
            }

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
            _input.Cancel();
            _model.TogglePilot(Time.time, _config.HoverAfterExitSeconds);
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

        public Vector3 GetCameraPosition(Transform eyes, CharacterController pilot, float blend)
        {
            var offset = (Vector3.up * _config.CameraHeight - eyes.forward * _config.CameraDistance) * blend;
            var distance = GetClearDistance(eyes.position, Vector3.one * Clearance, offset, pilot, Quaternion.identity);
            return eyes.position + offset.normalized * distance;
        }

        private void LateUpdate()
        {
            if (_input == null || !_input.IsEnabled)
            {
                SetRotorAudio(false);
                return;
            }

            var deltaTime = Time.deltaTime;
            var isFlying = _model.IsPiloted;
            if (_arrival != ArrivalPhase.Available)
            {
                isFlying = _arrival != ArrivalPhase.Waiting;
                if (isFlying)
                {
                    AdvanceArrival(deltaTime);
                }
            }
            else if (!isFlying)
            {
                if (_model.CanDescend(Time.time) && transform.position.y >= _origin.y - _config.FlightHeight)
                {
                    var descent = GetFlightDistance(Vector3.down * (_config.DescentSpeed * deltaTime), null);
                    transform.position += Vector3.down * descent;
                    isFlying = descent > 0f;
                }
                else
                {
                    isFlying = HasGroundClearance();
                }
            }

            SetRotorAudio(isFlying);
            if (isFlying && _rotorView != null)
            {
                _rotorView.Spin(deltaTime);
            }
        }

        internal bool CanLandAt(Vector3 point)
        {
            if (!WaitingForSignal || point.y >= CruiseY || Mathf.Abs(point.y - _origin.y) > _config.FlightHeight ||
                Vector3.ProjectOnPlane(point - _origin, Vector3.up).sqrMagnitude > _config.FlightRadius * _config.FlightRadius)
            {
                return false;
            }

            var half = GetHalfSize(_landingBounds);
            for (var x = -1; x <= 1; x += 2)
            {
                for (var z = -1; z <= 1; z += 2)
                {
                    var corner = point + transform.rotation * new Vector3(x * half.x, 0f, z * half.z);
                    if (!Physics.Raycast(corner + Vector3.up * 0.15f, Vector3.down, out var hit, 0.3f,
                            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) || hit.normal.y < 0.95f)
                    {
                        return false;
                    }
                }
            }

            var above = new Vector3(point.x, CruiseY, point.z);
            var start = new Vector3(transform.position.x, CruiseY, transform.position.z);
            return ClearLeg(transform.position, start) && ClearLeg(start, above) && ClearLeg(above, point);
        }

        private bool ClearLeg(Vector3 start, Vector3 end)
        {
            var delta = end - start;
            var rotation = transform.rotation;
            var distance = delta.magnitude;
            var upper = GetClearDistance(start + transform.TransformVector(_flightBounds.center),
                GetHalfSize(_flightBounds), delta, null, rotation);
            var lower = GetClearDistance(start + transform.TransformVector(_landingBounds.center),
                GetHalfSize(_landingBounds), delta, null, rotation);
            return Mathf.Min(upper, lower) + 0.002f >= distance;
        }

        internal void Dispatch(Vector3 point)
        {
            _landingPoint = point;
            _arrival = ArrivalPhase.Climb;
        }

        private void AdvanceArrival(float deltaTime)
        {
            var target = _landingPoint;
            var speed = _config.ApproachSpeed;
            if (_arrival == ArrivalPhase.Climb)
            {
                target = new Vector3(transform.position.x, CruiseY, transform.position.z);
            }
            else if (_arrival == ArrivalPhase.Approach)
            {
                target.y = CruiseY;
            }
            else
            {
                speed = _config.ArrivalDescentSpeed;
            }

            var delta = Vector3.MoveTowards(transform.position, target, speed * deltaTime) - transform.position;
            transform.position += delta.normalized * GetFlightDistance(delta, null);
            if ((transform.position - target).sqrMagnitude < 0.000025f)
            {
                switch (_arrival)
                {
                    case ArrivalPhase.Climb: _arrival = ArrivalPhase.Approach; break;
                    case ArrivalPhase.Approach: _arrival = ArrivalPhase.Descend; break;
                    case ArrivalPhase.Descend: _arrival = ArrivalPhase.Available; break;
                }
            }
        }

        private void OnDisable()
        {
            SetRotorAudio(false);
        }

        private void SetRotorAudio(bool isFlying)
        {
            if (_rotorAudio == null || _rotorAudio.clip == null || _rotorAudio.isPlaying == isFlying)
            {
                return;
            }

            if (isFlying)
            {
                _rotorAudio.Play();
            }
            else
            {
                _rotorAudio.Stop();
            }
        }

        private bool HasGroundClearance()
        {
            var half = GetHalfSize(_landingBounds);
            var center = transform.TransformPoint(_landingBounds.center);
            center.y -= half.y - Clearance;
            half.y = Clearance;
            return GetClearDistance(center, half, Vector3.down * Clearance, null, transform.rotation) > 0f;
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
