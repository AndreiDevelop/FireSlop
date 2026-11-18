using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.MissionHud;
using FireSlop.Tools;
using UnityEngine;
using Zenject;

namespace FireSlop.Drone
{
    public sealed class SignalThrowController : MonoBehaviour
    {
        private const int Samples = 81;
        private const float Step = 0.05f;
        private const float Radius = 0.06f;
        [SerializeField] private LineRenderer _arc;
        [SerializeField] private LineRenderer _marker;
        [SerializeField] private ParticleSystem _signalPrefab;

        private readonly Vector3[] _points = new Vector3[Samples];
        private readonly RaycastHit[] _hits = new RaycastHit[16];
        private DroneController _drone;
        private MissionInputModel _input;
        private InteractionModel _interaction;
        private ToolSelectionModel _tools;
        private Transform _eyes;
        private Transform _player;
        private Vector3 _landing;
        private int _count;
        private ParticleSystem _signal;
        private float _elapsed;
        private bool _focused = true;

        [Inject]
        public void Construct(Camera camera, DroneController drone, MissionInputModel input,
            InteractionModel interaction, ToolSelectionModel tools)
        {
            _eyes = camera.transform;
            _player = camera.GetComponentInParent<CharacterController>().transform;
            _drone = drone;
            _input = input;
            _interaction = interaction;
            _tools = tools;
            _interaction.SignalThrowRequested += Throw;
        }

        private bool CanAim => _focused && _input.IsEnabled && !_interaction.IsCarrying && !_interaction.IsPilotingDrone &&
            _drone.WaitingForSignal && _tools.SelectedAction == ToolAction.SignalCanister && _tools.IsAvailable(_tools.SelectedId);

        private void LateUpdate()
        {
            if (_signal != null && _count > 0 && _input.IsEnabled)
            {
                _elapsed += Time.deltaTime;
                var sample = _elapsed / Step;
                var index = Mathf.FloorToInt(sample);
                if (index >= _count - 1)
                {
                    _signal.transform.position = _points[_count - 1];
                    _signal.Play();
                    _drone.Dispatch(_landing);
                    _count = 0;
                }
                else
                {
                    _signal.transform.position = Vector3.Lerp(_points[index], _points[index + 1], sample - index);
                }
            }

            if (_signal != null && _count == 0 && _drone.HasArrived)
            {
                _signal.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(_signal.gameObject, _signal.main.startLifetime.constantMax);
                _signal = null;
            }

            var aim = _signal == null && _tools != null && CanAim;
            _arc.enabled = aim;
            _marker.enabled = aim && Trace();
            if (aim)
            {
                _arc.positionCount = _count;
                // SetPositions accepts a buffer larger than positionCount.
                _arc.SetPositions(_points);
            }
        }

        private bool Trace()
        {
            var start = _eyes.TransformPoint(_drone.Config.ThrowOffset);
            if (FindContact(_eyes.position, start, out var obstruction))
            {
                _points[0] = _eyes.position;
                _points[1] = obstruction.point;
                _count = 2;
                return false;
            }

            var velocity = _eyes.forward * _drone.Config.ThrowSpeed + Vector3.up * _drone.Config.ThrowLift;
            _points[0] = start;
            for (var i = 1; i < Samples; i++)
            {
                var time = i * Step;
                var next = start + velocity * time + Physics.gravity * (0.5f * time * time);
                if (FindContact(_points[i - 1], next, out var hit))
                {
                    _points[i] = hit.point + hit.normal * Radius;
                    _count = i + 1;
                    _landing = hit.point;
                    if (hit.normal.y < 0.95f || hit.rigidbody != null || !_drone.CanLandAt(_landing))
                    {
                        return false;
                    }

                    var half = _drone.Footprint;
                    var center = _landing + Vector3.up * 0.04f;
                    for (var corner = 0; corner < 4; corner++)
                    {
                        var x = corner < 2 ? -half.x : half.x;
                        var z = corner == 0 || corner == 3 ? -half.z : half.z;
                        _marker.SetPosition(corner, center + _drone.transform.rotation * new Vector3(x, 0f, z));
                    }

                    return true;
                }

                _points[i] = next;
            }

            _count = Samples;
            return false;
        }

        private bool FindContact(Vector3 start, Vector3 end, out RaycastHit contact)
        {
            var delta = end - start;
            var hits = Physics.SphereCastNonAlloc(start, Radius, delta.normalized, _hits, delta.magnitude,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            var distance = float.PositiveInfinity;
            contact = default;
            for (var i = 0; i < hits; i++)
            {
                var hit = _hits[i];
                if (hit.distance < distance && !hit.collider.transform.IsChildOf(_player))
                {
                    contact = hit;
                    distance = hit.distance;
                }
            }

            return distance < float.PositiveInfinity;
        }

        private void OnApplicationFocus(bool focused)
        {
            _focused = focused;
        }

        private void Throw()
        {
            if (_signal != null || !CanAim || !Trace() || !_tools.TryCommitUse(ToolAction.SignalCanister))
            {
                return;
            }

            _signal = Instantiate(_signalPrefab, _points[0], Quaternion.identity);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_signal.gameObject, gameObject.scene);
            _marker.enabled = false;
            _arc.enabled = false;
        }

        private void OnDestroy()
        {
            if (_interaction != null)
            {
                _interaction.SignalThrowRequested -= Throw;
            }
        }
    }
}
