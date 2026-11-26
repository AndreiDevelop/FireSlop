using System.Collections.Generic;
using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class HoseStreamView : MonoBehaviour
    {
        private const int Sides = 12;
        private const int IntervalsPerPacket = 4;
        private const int BendSegments = 5;
        private const int RingStride = Sides + 1;
        private const int SampleCapacity = HoseController.MaxPackets * (IntervalsPerPacket + 2);
        private const float TimeTolerance = 0.00001f;

        [SerializeField] private MeshFilter _body;
        [SerializeField] private MeshRenderer _bodyRenderer;
        [SerializeField, Min(0.001f)] private float _nozzleRadius = 0.035f;
        [SerializeField, Min(0.001f)] private float _farRadius = 0.065f;
        [SerializeField, Min(0.01f)] private float _flowRepeatMeters = 1.7f;
        [SerializeField] private HoseSprayView _spray;
        [SerializeField, Range(0.01f, 0.9f)] private float _breakupDuration = 0.32f;
        [SerializeField, Min(0.01f)] private float _releaseTaperSeconds = 0.09f;
        [SerializeField] private ParticleSystem _splash;

        private readonly List<Vector3> _vertices = new List<Vector3>(SampleCapacity * RingStride);
        private readonly List<Vector2> _uvs = new List<Vector2>(SampleCapacity * RingStride);
        private readonly List<int> _indices = new List<int>(SampleCapacity * Sides * 6);
        private readonly Vector4[] _samples = new Vector4[SampleCapacity];
        private readonly Vector2[] _circle = new Vector2[RingStride];
        private int _sampleCount;
        private Vector4 _pathEnd;
        private float _sampleInterval;
        private float _launchSpeed;
        private float _flightLifetime;
        private float _coreLifetime;
        private float _tailAge;
        private Vector3 _previousRight;
        private Matrix4x4 _worldToLocal;
        private Vector3 _eyePosition;
        private Mesh _mesh;
        private float _splashRate;
        private float _splashRemainder;

        private void Awake()
        {
            if (_body == null || _bodyRenderer == null || _spray == null || _splash == null)
            {
                Debug.LogError("Hose stream requires an authored mesh renderer, filter and impact particles.", this);
                enabled = false;
                return;
            }

            _mesh = new Mesh { name = "Hose stream" };
            _mesh.MarkDynamic();
            _body.sharedMesh = _mesh;
            _bodyRenderer.enabled = false;
            for (var side = 0; side <= Sides; side++)
            {
                var angle = side * (2f * Mathf.PI / Sides);
                _circle[side] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }

            var emission = _splash.emission;
            _splashRate = emission.rateOverTime.Evaluate(0f, 0.5f);
            emission.enabled = false;
        }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                Destroy(_mesh);
            }
        }

        internal void BeginFrame(Vector3 eyePosition, float packetInterval, float flightLifetime)
        {
            _spray.BeginFrame();
            _flightLifetime = flightLifetime;
            // The last third of the breakup interval contains only airborne drops.
            _coreLifetime = flightLifetime * (1f - _breakupDuration / 3f);
            _sampleCount = 0;
            _indices.Clear();
            _vertices.Clear();
            _uvs.Clear();
            _eyePosition = eyePosition;
            _sampleInterval = packetInterval / IntervalsPerPacket;
            if (_body != null)
            {
                _worldToLocal = _body.transform.worldToLocalMatrix;
            }
        }

        internal void DrawPacket(HoseWaterPacket packet, Vector3 gravity, float age)
        {
            var youngest = Mathf.Max(0f, age - packet.EmissionDuration);
            var oldest = Mathf.Min(age, packet.FlightLimit);
            if (packet.TracksNearContact && (packet.HasImpacted || packet.HasPendingImpact))
            {
                var arrival = age - packet.FlightLimit;
                var end = Mathf.Clamp(arrival, 0f, packet.EmissionDuration);
                var arrivalStart = Mathf.Clamp(arrival - Time.deltaTime, 0f, packet.EmissionDuration);
                if (end > arrivalStart)
                {
                    var point = packet.StreamPosition(gravity, packet.FlightLimit + end, packet.FlightLimit);
                    Splash(point, packet.ContactNormal, end - arrivalStart);
                }
            }

            if (!isActiveAndEnabled || oldest <= youngest)
            {
                return;
            }

            _spray.Sample(packet, gravity, age, _flightLifetime);
            oldest = Mathf.Min(oldest, _coreLifetime);
            if (oldest <= youngest)
            {
                return;
            }

            _launchSpeed = packet.LaunchVelocity.magnitude;
            var start = SamplePacket(packet, gravity, age, youngest);
            if (_sampleCount > 0 && HasGapBefore(start))
            {
                // A clipped packet or a released trigger can leave a real gap in emission time.
                FlushPath();
            }

            if (_sampleCount == 0)
            {
                _samples[_sampleCount++] = start;
            }

            // Fix interior rings to flight ages measured from the outlet. Physics packet boundaries move
            // through this grid each frame and must never introduce a ring or a new smoothing control point.
            var first = Mathf.CeilToInt((youngest - TimeTolerance) / _sampleInterval);
            for (var index = first; index * _sampleInterval <= oldest + TimeTolerance; index++)
            {
                var flightAge = Mathf.Clamp(index * _sampleInterval, youngest, oldest);
                if (flightAge - _samples[_sampleCount - 1].w > TimeTolerance)
                {
                    _samples[_sampleCount++] = SamplePacket(packet, gravity, age, flightAge);
                }
            }

            _pathEnd = SamplePacket(packet, gravity, age, oldest);
        }

        private bool HasGapBefore(Vector4 start)
        {
            Vector3 offset = start - _pathEnd;
            return Mathf.Abs(start.w - _pathEnd.w) > TimeTolerance || offset.sqrMagnitude > _farRadius * _farRadius;
        }

        private static Vector4 SamplePacket(HoseWaterPacket packet, Vector3 gravity, float age, float flightAge)
        {
            var center = packet.StreamPosition(gravity, age, flightAge);
            return new Vector4(center.x, center.y, center.z, flightAge);
        }

        private void FlushPath()
        {
            if (_sampleCount > 0 && _pathEnd.w - _samples[_sampleCount - 1].w > TimeTolerance)
            {
                _samples[_sampleCount++] = _pathEnd;
            }

            if (_sampleCount >= 2)
            {
                var previous = _samples[0];
                _tailAge = previous.w;
                var current = SmoothedSample(1);
                AppendRing(previous, current - previous, false);
                if (_sampleCount > 2)
                {
                    AppendRing((previous + current) * 0.5f, current - previous, true);
                }

                for (var i = 1; i < _sampleCount - 1; i++)
                {
                    var next = SmoothedSample(i + 1);
                    AppendBend(previous, current, next);
                    previous = current;
                    current = next;
                }

                AppendRing(current, current - previous, true);
            }

            _sampleCount = 0;
        }

        private Vector4 SmoothedSample(int index)
        {
            var last = _sampleCount - 1;
            if (index == last)
            {
                return _samples[last];
            }

            // Spread abrupt changes in aim over neighbouring samples; the outlet and hit stay pinned.
            var outer = _samples[Mathf.Max(0, index - 2)] + _samples[Mathf.Min(last, index + 2)];
            var inner = _samples[index - 1] + _samples[index + 1];
            return (outer + 4f * inner + 6f * _samples[index]) / 16f;
        }

        private void AppendBend(Vector4 previous, Vector4 corner, Vector4 next)
        {
            // Adjacent quadratic spans share a midpoint and tangent, including across packet boundaries.
            var start = (previous + corner) * 0.5f;
            var end = (corner + next) * 0.5f;
            Vector3 incoming = corner - start;
            Vector3 outgoing = end - corner;
            var dot = Vector3.Dot(incoming, outgoing);
            var straight = dot > 0f && dot * dot > 0.999f * incoming.sqrMagnitude * outgoing.sqrMagnitude;
            if (straight)
            {
                AppendRing(end, outgoing, true);
                return;
            }

            for (var step = 1; step <= BendSegments; step++)
            {
                var t = (float)step / BendSegments;
                var left = Vector4.LerpUnclamped(start, corner, t);
                var right = Vector4.LerpUnclamped(corner, end, t);
                AppendRing(Vector4.LerpUnclamped(left, right, t), right - left, true);
            }
        }

        private void AppendRing(Vector4 point, Vector3 tangent, bool connect)
        {
            Vector3 center = point;
            var direction = tangent.normalized;
            // Orient the transverse UV axis across the visible stream, even when it swings around the player.
            var right = Vector3.Cross(_eyePosition - center, direction);
            if (right.sqrMagnitude < 0.000001f)
            {
                right = Vector3.ProjectOnPlane(_body.transform.right, direction);
                if (right.sqrMagnitude < 0.000001f)
                {
                    right = Vector3.ProjectOnPlane(_body.transform.up, direction);
                }
            }

            right.Normalize();
            if (connect && Vector3.Dot(right, _previousRight) < 0f)
            {
                right = -right;
            }

            _previousRight = right;
            var radius = Mathf.Lerp(_nozzleRadius, _farRadius, point.w);
            var taperDuration = _coreLifetime - _flightLifetime * (1f - _breakupDuration);
            radius *= Mathf.SmoothStep(0f, 1f, (_coreLifetime - point.w) / taperDuration);
            if (_tailAge > 0f)
            {
                var tail = Mathf.SmoothStep(0f, 1f, (point.w - _tailAge) / _releaseTaperSeconds);
                radius *= Mathf.Lerp(1f, tail, _tailAge / _releaseTaperSeconds);
            }

            var up = _worldToLocal.MultiplyVector(Vector3.Cross(direction, right) * radius);
            right = _worldToLocal.MultiplyVector(right * radius);
            center = _worldToLocal.MultiplyPoint3x4(center);
            var uvY = -point.w * _launchSpeed / _flowRepeatMeters;
            var first = _vertices.Count;
            for (var side = 0; side <= Sides; side++)
            {
                _vertices.Add(center + right * _circle[side].x + up * _circle[side].y);
                _uvs.Add(new Vector2(0.5f + _circle[side].x * 0.5f, uvY));
                if (connect && side < Sides)
                {
                    var current = first + side;
                    var previous = current - RingStride;
                    _indices.Add(previous);
                    _indices.Add(previous + 1);
                    _indices.Add(current);
                    _indices.Add(previous + 1);
                    _indices.Add(current + 1);
                    _indices.Add(current);
                }
            }
        }

        internal void DrawFrame()
        {
            if (_mesh == null)
            {
                return;
            }

            _spray.DrawFrame();
            FlushPath();
            if (_vertices.Count == 0)
            {
                _bodyRenderer.enabled = false;
                return;
            }

            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetTriangles(_indices, 0);
            _bodyRenderer.enabled = true;
        }

        public void Splash(Vector3 point, Vector3 normal, float sampleDuration)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            _splash.transform.SetPositionAndRotation(point, Quaternion.FromToRotation(Vector3.forward, normal));
            _splashRemainder += _splashRate * sampleDuration;
            var count = Mathf.FloorToInt(_splashRemainder);
            _splashRemainder -= count;
            if (count > 0)
            {
                _splash.Emit(count);
            }
        }

        public void Hide()
        {
            ClearStream();
            _splashRemainder = 0f;
            if (_splash != null)
            {
                _splash.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        internal void ClearStream()
        {
            if (_spray != null)
            {
                _spray.Clear();
            }

            if (_bodyRenderer != null)
            {
                _bodyRenderer.enabled = false;
            }
        }
    }
}
