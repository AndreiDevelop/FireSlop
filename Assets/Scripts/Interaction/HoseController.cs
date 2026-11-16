using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    [DefaultExecutionOrder(110)]
    public sealed class HoseController : MonoBehaviour
    {
        [SerializeField] private Transform _nozzle;
        [SerializeField] private HoseStreamView _stream;
        [SerializeField] private HoseConfigSO _config;

        private Transform _aim;
        private InteractionModel _input;

        [Inject]
        public void Construct(Camera camera, InteractionModel input)
        {
            _aim = camera.transform;
            _input = input;
        }

        private void Awake()
        {
            if (_nozzle == null || _stream == null || _config == null || _config.Range <= 0f || _config.Force <= 0f)
            {
                Debug.LogError("Hose requires a nozzle, stream view, and positive range and force settings.", this);
                enabled = false;
            }
        }

        private void OnDisable()
        {
            if (_stream != null)
            {
                _stream.Hide();
            }
        }

        private void FixedUpdate()
        {
            if (_input == null || !_input.HoseActive || !TraceStream(out var origin, out _, out var hit))
            {
                return;
            }

            var body = hit.rigidbody;
            if (body != null && !body.isKinematic)
            {
                var direction = (hit.point - origin).normalized;
                body.AddForceAtPosition(direction * _config.Force, hit.point, ForceMode.Force);
            }
        }

        private void LateUpdate()
        {
            if (_input != null && _input.HoseActive)
            {
                var hasHit = TraceStream(out var origin, out var end, out var hit);
                _stream.Render(origin, end, hasHit ? hit.normal : Vector3.zero);
            }
        }

        private bool TraceStream(out Vector3 origin, out Vector3 end, out RaycastHit hit)
        {
            var sightOrigin = _aim.position;
            origin = _nozzle.position;
            var sightHit = Raycast(sightOrigin, _aim.forward, _config.Range, out hit);
            end = sightHit ? hit.point : sightOrigin + _aim.forward * _config.Range;

            var toNozzle = origin - sightOrigin;
            if (Raycast(sightOrigin, toNozzle.normalized, toNozzle.magnitude, out var nearHit))
            {
                origin = sightOrigin;
                hit = nearHit;
                end = nearHit.point;
                return true;
            }

            var toEnd = end - origin;
            if (Raycast(origin, toEnd.normalized, toEnd.magnitude, out var nozzleHit))
            {
                hit = nozzleHit;
                end = nozzleHit.point;
                return true;
            }

            return sightHit;
        }

        private static bool Raycast(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit)
        {
            return Physics.Raycast(origin, direction, out hit, distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }
    }
}
