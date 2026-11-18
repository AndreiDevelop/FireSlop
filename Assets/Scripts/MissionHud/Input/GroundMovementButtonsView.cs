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
        [SerializeField] private Button _jumpButton;

        private DroneModel _flight;
        private MissionInputModel _input;

        [Inject]
        public void Construct(DroneModel flight, MissionInputModel input)
        {
            if (!_controls || !_jumpButton)
            {
                Debug.LogError("Ground Movement requires a controls group and jump button.", this);
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
            _jumpButton.onClick.RemoveListener(_input.RequestJump);
            _controls.SetActive(false);
        }

        private void Bind()
        {
            _flight.ContextChanged += Render;
            _jumpButton.onClick.AddListener(_input.RequestJump);
            Render();
        }

        private void Render()
        {
            _controls.SetActive(MissionHudPlatform.UsesTouchControls && !_flight.IsPiloted);
        }
    }
}
