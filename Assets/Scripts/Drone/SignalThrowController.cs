using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.MissionHud;
using FireSlop.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace FireSlop.Drone
{
    public sealed class SignalThrowController : MonoBehaviour
    {
        [SerializeField] private ThrowArcView _trajectory;
        [SerializeField] private Transform _landingPreview;
        [SerializeField] private ParticleSystem _signalPrefab;

        private DroneController _drone;
        private MissionInputModel _input;
        private InteractionModel _interaction;
        private ToolSelectionModel _tools;
        private ThrowPoseView _pose;
        private Vector3 _landing;
        private ParticleSystem _signal;
        private float _elapsed;
        private bool _inFlight;
        private bool _focused = true;

        [Inject]
        public void Construct(DroneController drone, MissionInputModel input, InteractionModel interaction,
            ToolSelectionModel tools, ThrowPoseView pose)
        {
            _drone = drone;
            _input = input;
            _interaction = interaction;
            _tools = tools;
            _pose = pose;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private bool CanAim => _focused && _input.IsEnabled && !_interaction.IsCarrying && !_interaction.IsPilotingDrone &&
            _drone.WaitingForSignal && _tools.SelectedAction == ToolAction.SignalCanister && _tools.IsAvailable(_tools.SelectedId);

        private void OnEnable()
        {
            if (_interaction != null)
            {
                Subscribe();
            }
        }

        private void Subscribe()
        {
            _interaction.InventoryThrowRequested -= Throw;
            _interaction.InventoryThrowRequested += Throw;
        }

        private void LateUpdate()
        {
            AdvanceSignal();
            var aim = _signal == null && _tools != null && CanAim;
            _trajectory.SetVisible(aim);
            var canThrow = aim && Trace();
            _landingPreview.gameObject.SetActive(canThrow);
            if (aim)
            {
                _trajectory.SetValidity(canThrow);
            }
        }

        private void AdvanceSignal()
        {
            if (_signal != null && _inFlight && _input.IsEnabled)
            {
                _elapsed += Time.deltaTime;
                _signal.transform.position = _trajectory.Path.PositionAt(_elapsed);
                if (_elapsed >= _trajectory.Path.Duration)
                {
                    _signal.Play();
                    _drone.Dispatch(_landing);
                    _inFlight = false;
                }
            }

            if (_signal != null && !_inFlight && _drone.HasArrived)
            {
                _signal.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(_signal.gameObject, _signal.main.startLifetime.constantMax);
                _signal = null;
            }
        }

        private bool Trace()
        {
            _trajectory.Trace(_pose);
            var path = _trajectory.Path;
            if (!path.IsOriginClear || !path.HasContact)
            {
                return false;
            }

            var hit = path.Contact;
            _landing = hit.point;
            if (hit.normal.y < 0.95f || hit.rigidbody != null || !_drone.CanLandAt(_landing))
            {
                return false;
            }

            _landingPreview.SetPositionAndRotation(_landing, _drone.transform.rotation);
            return true;
        }

        private void OnApplicationFocus(bool focused)
        {
            _focused = focused;
        }

        private void Throw(ToolAction action)
        {
            if (action != ToolAction.SignalCanister || _signal != null || !CanAim || !Trace() || !_tools.TryCommitUse(ToolAction.SignalCanister))
            {
                return;
            }

            _signal = Instantiate(_signalPrefab, _pose.LaunchPosition, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(_signal.gameObject, gameObject.scene);
            _elapsed = 0f;
            _inFlight = true;
            _landingPreview.gameObject.SetActive(false);
            _trajectory.SetVisible(false);
        }

        private void OnDisable()
        {
            if (_interaction != null)
            {
                _interaction.InventoryThrowRequested -= Throw;
            }

            if (_trajectory != null)
            {
                _trajectory.SetVisible(false);
            }

            if (_landingPreview != null)
            {
                _landingPreview.gameObject.SetActive(false);
            }
        }
    }
}
