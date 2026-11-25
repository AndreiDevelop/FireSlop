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

        [SerializeField] private AudioSource _sprayAudio;

        private readonly HoseWaterPacket[] _packets = new HoseWaterPacket[MaxPackets];
        private Transform _aim;
        private Vector3 _eyeLocalPosition;
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
            _eyeLocalPosition = _aim.localPosition;
            _input = input;
            SubscribeToHose();
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
            SubscribeToHose();
        }

        private void SubscribeToHose()
        {
            if (_input == null || !isActiveAndEnabled)
            {
                return;
            }

            _input.HoseStarted += StartSprayAudio;
            _input.HoseStopped += StopEmission;
            if (_input.HoseActive)
            {
                StartSprayAudio();
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.HoseStarted -= StartSprayAudio;
                _input.HoseStopped -= StopEmission;
                _input.SetHoseAim(false);
            }

            StopSprayAudio();
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

        private void StartSprayAudio()
        {
            if (_sprayAudio != null && _sprayAudio.clip != null)
            {
                _sprayAudio.Play();
            }
        }

        private void StopSprayAudio()
        {
            if (_sprayAudio != null)
            {
                _sprayAudio.Stop();
            }
        }

        private void StopEmission()
        {
            StopSprayAudio();
            if (_packetCount > 0)
            {
                _packets[_packetCount - 1].UpdateEmissionEnd(
                    _nozzle.position, GetLaunchVelocity(), Time.timeAsDouble, true);
            }

            for (var index = 0; index < _packetCount; index++)
            {
                _packets[index].AffectsTargets = false;
            }

            _stream.EndEmission();
        }

        private void FixedUpdate()
        {
            if (_input == null)
            {
                return;
            }

            using (BallisticsMarker.Auto())
            {
                var time = Time.fixedTimeAsDouble;
                var sampleDuration = Time.fixedDeltaTime * _stepsBetweenSamples;
                var canAim = _input.CanSprayHose && Application.isFocused;
                _input.SetHoseAim(canAim && IsAimingAtFire(sampleDuration));
                if (!_input.HoseActive && _packetCount == 0)
                {
                    return;
                }

                var advance = _stepsUntilLaunch == 0;
                AdvancePackets(time, sampleDuration, advance);
                if (advance && _input.HoseActive)
                {
                    LaunchPacket(time, sampleDuration);
                }

                if (!_input.HoseActive && _packetCount == 0)
                {
                    _stream.ClearDroplets();
                    _stepsUntilLaunch = 0;
                    return;
                }

                _stepsUntilLaunch = advance ? _stepsBetweenSamples - 1 : _stepsUntilLaunch - 1;
            }
        }

        private void AdvancePackets(double time, float sampleDuration, bool advance)
        {
            var retainedCount = 0;
            for (var index = 0; index < _packetCount; index++)
            {
                ref var packet = ref _packets[index];
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

                if (retainedCount != index)
                {
                    _packets[retainedCount] = packet;
                }

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
            if (packet.AffectsTargets)
            {
                ApplyImpact(hit, direction, sampleDuration, packet.CanPush);
            }

            _stream.Splash(hit.point, hit.normal, sampleDuration);
        }

        private bool FindImpact(in HoseWaterPacket packet, LayerMask mask, out RaycastHit hit, out float age)
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

            var packet = CreatePacket(time);
            packet.CanPush = _input.IsHoseInUseManually;
            packet.DropletCount = _stream.TakeDroplets(sampleDuration);
            if (_packetCount > 0)
            {
                _packets[_packetCount - 1].UpdateEmissionEnd(packet.Origin, packet.LaunchVelocity, time, true);
            }

            PreviewSegment(ref packet, sampleDuration);
            _packets[_packetCount] = packet;
            _packetCount++;
        }

        private bool IsAimingAtFire(float sampleDuration)
        {
            var packet = CreatePacket(Time.fixedTimeAsDouble);
            while (packet.CheckedAge < _flightLimit)
            {
                packet.SegmentStartAge = packet.CheckedAge;
                packet.CheckedAge = Mathf.Min(packet.CheckedAge + sampleDuration, _flightLimit);
                if (FindImpact(packet, _config.CollisionMask, out var hit, out _))
                {
                    return TryGetAutoSprayTarget(hit.collider, out var target) && target.RequestsAutoSpray;
                }
            }

            return false;
        }

        private HoseWaterPacket CreatePacket(double time)
        {
            var origin = _nozzle.position;
            var eyePosition = _aim.parent.TransformPoint(_eyeLocalPosition);
            var toNozzle = origin - eyePosition;
            var obstructed = Cast(eyePosition, toNozzle, _config.CollisionMask, out var obstruction);
            if (obstructed)
            {
                // A nozzle clipping a wall must emit from the player's side.
                origin = eyePosition + toNozzle.normalized * Mathf.Max(0f, obstruction.distance - SurfaceClearance);
            }

            return new HoseWaterPacket(origin, GetLaunchVelocity(), time, _flightLimit)
            {
                NozzleObstructed = obstructed
            };
        }

        private bool Cast(Vector3 origin, Vector3 travel, LayerMask mask, out RaycastHit hit)
        {
            hit = default;
            var length = travel.magnitude;
            return length > 0f && Physics.SphereCast(origin, _config.CastRadius, travel / length, out hit, length,
                mask, QueryTriggerInteraction.Collide);
        }

        private bool TryGetAutoSprayTarget(Collider collider, out IAutoSprayTarget target)
        {
            target = null;
            return (_config.FireMask.value & (1 << collider.gameObject.layer)) != 0 &&
                collider.TryGetComponent(out target);
        }

        private void ApplyImpact(RaycastHit hit, Vector3 direction, float sampleDuration, bool canPush)
        {
            var layerBit = 1 << hit.collider.gameObject.layer;
            var impulse = canPush && (_config.PushMask.value & layerBit) != 0 ? _config.Force * sampleDuration : 0f;
            var impact = new WaterImpact(hit, direction, sampleDuration, impulse);
            if ((_config.FireMask.value & layerBit) != 0 && hit.collider.TryGetComponent<IAutoSprayTarget>(out var sprayTarget))
            {
                sprayTarget.ApplyWater(impact);
                return;
            }

            if (impulse <= 0f)
            {
                return;
            }

            var target = hit.collider.GetComponentInParent<IWaterTarget>();
            if (target != null)
            {
                target.ApplyWater(impact);
            }
            else
            {
                // A dynamic body on a push layer also responds without a custom target.
                WaterPushable.PushHitBody(impact);
            }
        }

        private Vector3 GetLaunchVelocity()
        {
            return _aim.forward * _config.LaunchSpeed;
        }

        private void LateUpdate()
        {
            if (_packetCount == 0)
            {
                return;
            }

            using (WaterRenderingMarker.Auto())
            {
                var time = Time.timeAsDouble;
                var sampleDuration = Time.fixedDeltaTime * _stepsBetweenSamples;
                _stream.BeginFrame();
                for (var index = _packetCount - 1; index >= 0; index--)
                {
                    var packet = _packets[index];
                    if (index == _packetCount - 1 && _input.HoseActive)
                    {
                        packet.UpdateEmissionEnd(_nozzle.position, GetLaunchVelocity(), time, false);
                    }

                    var age = (float)(time - packet.LaunchTime);
                    _stream.DrawPacket(packet, _gravity, age, sampleDuration);
                }

                _stream.DrawFrame();
            }
        }
    }
}
