using FireSlop.Drone;
using FireSlop.Firefighter;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class GroundMovementButtonsView : MonoBehaviour
    {
        [SerializeField] private GameObject _controls;
        [SerializeField] private GameObject _droneControls;
        [SerializeField] private Button _jumpButton;

        private DroneModel _flight;
        private MissionInputModel _input;

        private MissionHudPlatform _platform;

        [Inject]
        public void Construct(DroneModel flight, MissionInputModel input, MissionHudPlatform platform)
        {
            _platform = platform;
            if (!_controls || !_droneControls || !_jumpButton)
            {
                Debug.LogError("Movement buttons require ground controls, drone controls, and a jump button.", this);
                return;
            }

            _flight = flight;
            _input = input;
            if (isActiveAndEnabled)
            {
                Bind();
            }
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                Bind();
            }
        }

        private void OnDisable()
        {
            if (_input == null)
            {
                return;
            }

            _flight.ContextChanged -= Render;
            _platform.Changed -= Render;
            _jumpButton.onClick.RemoveListener(_input.RequestJump);
            _controls.SetActive(false);
            _droneControls.SetActive(false);
        }

        private void Bind()
        {
            _flight.ContextChanged += Render;
            _platform.Changed += Render;
            _jumpButton.onClick.AddListener(_input.RequestJump);
            Render();
        }

        private void Render()
        {
            var touch = _platform.UsesTouchControls;
            _controls.SetActive(touch && !_flight.IsPiloted);
            _droneControls.SetActive(touch && _flight.IsPiloted);
        }
    }
}
