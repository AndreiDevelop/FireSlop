using Unity.Profiling;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    [DefaultExecutionOrder(110)]
    public sealed class HoseController : MonoBehaviour
    {
        private const int MaxPackets = 32;
        private const float SurfaceClearance = 0.002f;
        private static readonly ProfilerMarker BallisticsMarker = new ProfilerMarker("Hose.Ballistics");
        private static readonly ProfilerMarker WaterRenderingMarker = new ProfilerMarker("Hose.WaterRendering");

        [SerializeField] private Transform _nozzle;
        [SerializeField] private HoseStreamView _stream;
        [SerializeField] private HoseConfigSO _config;

        private readonly HoseWaterPacket[] _packets = new HoseWaterPacket[MaxPackets];
        private Transform _aim;
        private InteractionModel _input;
        private Vector3 _gravity;
        private float _flightLimit;
        private int _packetCount;
        private int _stepsBetweenSamples;
        private int _stepsUntilLaunch;

        [Inject]
        public void Construct(Camera camera, InteractionModel input)
        {
            _aim = camera.transform;
            _input = input;
            if (isActiveAndEnabled)
            {
                _input.HoseStopped += ResetStream;
            }
        }

        private void Awake()
        {
            if (_nozzle == null || _stream == null || _config == null || !_config.IsValid)
            {
                Debug.LogError("Hose requires a nozzle, stream view, and valid trajectory and collision settings.", this);
                enabled = false;
                return;
            }

            _gravity = Physics.gravity * _config.GravityScale;
            _flightLimit = _config.Range / _config.LaunchSpeed;
            var minimumSteps = Mathf.CeilToInt(_flightLimit / (Time.fixedDeltaTime * (MaxPackets - 2)));
            _stepsBetweenSamples = Mathf.Max(_config.FixedStepsPerSample, minimumSteps);
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.HoseStopped += ResetStream;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.HoseStopped -= ResetStream;
            }

            ResetStream();
        }

        private void ResetStream()
        {
            _packetCount = 0;
            _stepsUntilLaunch = 0;
            if (_stream != null)
            {
                _stream.Hide();
            }
        }

        private void FixedUpdate()
        {
            if (_input == null || !_input.HoseActive)
            {
                return;
            }

            using (BallisticsMarker.Auto())
            {
                var time = Time.fixedTimeAsDouble;
                var sampleDuration = Time.fixedDeltaTime * _stepsBetweenSamples;
                var launch = _stepsUntilLaunch == 0;
                AdvancePackets(time, sampleDuration, launch);
                if (launch)
                {
                    _stepsUntilLaunch = _stepsBetweenSamples;
                    LaunchPacket(time, sampleDuration);
                }

                _stepsUntilLaunch--;
            }
        }

        private void AdvancePackets(double time, float sampleDuration, bool advance)
        {
            var retainedCount = 0;
            for (var index = 0; index < _packetCount; index++)
            {
                var packet = _packets[index];
                var age = (float)(time - packet.LaunchTime);
                if (packet.HasPendingImpact && age >= packet.FlightLimit)
                {
                    ResolveImpact(ref packet, age, sampleDuration);
                }

                if (advance && !packet.HasPendingImpact && !packet.HasImpacted)
                {
                    ConfirmMovingImpact(ref packet, sampleDuration);
                    if (!packet.HasImpacted)
                    {
                        PreviewSegment(ref packet, age + sampleDuration);
                    }
                }

                if (age >= packet.FlightLimit + sampleDuration)
                {
                    continue;
                }

                _packets[retainedCount] = packet;
                retainedCount++;
            }

            _packetCount = retainedCount;
        }

        private void PreviewSegment(ref HoseWaterPacket packet, float age)
        {
            age = Mathf.Min(age, _flightLimit);
            if (age <= packet.CheckedAge)
            {
                return;
            }

            packet.SegmentStartAge = packet.CheckedAge;
            packet.CheckedAge = age;
            if (FindImpact(packet, _config.CollisionMask, out _, out var impactAge))
            {
                packet.FlightLimit = impactAge;
                packet.HasPendingImpact = true;
            }
        }

        private void ResolveImpact(ref HoseWaterPacket packet, float age, float sampleDuration)
        {
            // Recheck the short segment at arrival so a moving leaf or prop cannot leave a stale hit.
            if (!FindImpact(packet, _config.CollisionMask, out var hit, out var impactAge))
            {
                packet.FlightLimit = _flightLimit;
                packet.HasPendingImpact = false;
                return;
            }

            packet.FlightLimit = impactAge;
            if (age < impactAge)
            {
                return;
            }

            FinishImpact(ref packet, hit, impactAge, sampleDuration);
        }

        private void ConfirmMovingImpact(ref HoseWaterPacket packet, float sampleDuration)
        {
            if (packet.ConfirmedAge >= packet.CheckedAge)
            {
                return;
            }

            packet.ConfirmedAge = packet.CheckedAge;
            // A moving collider can enter a segment after it was queried for the visible stream.
            if (FindImpact(packet, _config.MovingCollisionMask, out var hit, out var impactAge))
            {
                FinishImpact(ref packet, hit, impactAge, sampleDuration);
            }
        }

        private void FinishImpact(ref HoseWaterPacket packet, RaycastHit hit, float age, float sampleDuration)
        {
            packet.FlightLimit = age;
            packet.HasPendingImpact = false;
            packet.HasImpacted = true;
            var direction = packet.VelocityAt(age, _gravity).normalized;
            ApplyImpact(hit, direction, sampleDuration);
        }

        private bool FindImpact(HoseWaterPacket packet, LayerMask mask, out RaycastHit hit, out float age)
        {
            age = 0f;
            var start = packet.PositionAt(packet.SegmentStartAge, _gravity);
            var travel = packet.PositionAt(packet.CheckedAge, _gravity) - start;
            if (!Cast(start, travel, mask, out hit))
            {
                return false;
            }

            age = Mathf.Lerp(packet.SegmentStartAge, packet.CheckedAge, Mathf.Clamp01(hit.distance / travel.magnitude));
            return true;
        }

        private void LaunchPacket(double time, float sampleDuration)
        {
            if (_packetCount == MaxPackets)
            {
                return;
            }

            var origin = _nozzle.position;
            var toNozzle = origin - _aim.position;
            if (Cast(_aim.position, toNozzle, _config.CollisionMask, out var obstruction))
            {
                // A nozzle clipping a wall must emit from the player's side.
                origin = _aim.position + toNozzle.normalized * Mathf.Max(0f, obstruction.distance - SurfaceClearance);
            }

            var velocity = _aim.forward * _config.LaunchSpeed;
            var packet = new HoseWaterPacket(origin, velocity, time, _flightLimit);
            PreviewSegment(ref packet, sampleDuration);
            _packets[_packetCount] = packet;
            _packetCount++;
        }

        private bool Cast(Vector3 origin, Vector3 travel, LayerMask mask, out RaycastHit hit)
        {
            hit = default;
            var length = travel.magnitude;
            return length > 0f && Physics.SphereCast(origin, _config.CastRadius, travel / length, out hit, length,
                mask, QueryTriggerInteraction.Collide);
        }

        private void ApplyImpact(RaycastHit hit, Vector3 direction, float sampleDuration)
        {
            var layerBit = 1 << hit.collider.gameObject.layer;
            if ((_config.FireMask.value & layerBit) != 0 &&
                hit.collider.TryGetComponent<WaterFireTarget>(out var fire))
            {
                fire.ApplyWater(sampleDuration);
            }
            else if ((_config.PushMask.value & layerBit) != 0)
            {
                var body = hit.rigidbody;
                if (body != null && !body.isKinematic)
                {
                    body.AddForceAtPosition(direction * (_config.Force * sampleDuration), hit.point, ForceMode.Impulse);
                }
            }

            _stream.Splash(hit.point, hit.normal, sampleDuration);
        }

        private void LateUpdate()
        {
            if (_input == null || !_input.HoseActive)
            {
                return;
            }

            using (WaterRenderingMarker.Auto())
            {
                var time = Time.timeAsDouble;
                var sampleDuration = Time.fixedDeltaTime * _stepsBetweenSamples;
                _stream.BeginFrame();
                for (var index = 0; index < _packetCount; index++)
                {
                    var packet = _packets[index];
                    var age = (float)(time - packet.LaunchTime);
                    _stream.DrawPacket(packet, _gravity, age, sampleDuration);
                }

                _stream.DrawFrame();
            }
        }
    }
}
