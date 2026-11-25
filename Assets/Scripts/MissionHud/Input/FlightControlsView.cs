using FireSlop.Drone;
using UnityEngine;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class FlightControlsView : MonoBehaviour
    {
        [SerializeField] private GameObject _body;
        [SerializeField] private bool _inFlight;

        [SerializeField] private bool _requiresTools;

        private ToolSelectionModel _tools;
        private DroneModel _model;

        [Inject]
        public void Construct(DroneModel drone, ToolSelectionModel tools)
        {
            _tools = tools;
            _model = drone;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void OnEnable()
        {
            if (_model != null)
            {
                _model.ContextChanged += Render;
                _tools.LockChanged += Render;
                Render();
            }
        }

        private void OnDisable()
        {
            if (_model != null)
            {
                _model.ContextChanged -= Render;
                _tools.LockChanged -= Render;
            }
        }

        private void Render()
        {
            _body.SetActive(_model.IsPiloted == _inFlight && (!_requiresTools || !_tools.IsLocked));
        }
    }
}
