using FireSlop.Drone;
using FireSlop.Firefighter;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class DroneElevationView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private bool _ascend;
        [SerializeField] private Text _label;

        private string _actionLabel;
        private DroneModel _drone;
        private MissionInputModel _input;
        private int? _pointer;

        [Inject]
        public void Construct(DroneModel drone, MissionInputModel input)
        {
            _drone = drone;
            _input = input;
            _actionLabel = _label.text;
            _drone.ContextChanged += Render;
            _input.Cancelled += Render;
            Render();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointer.HasValue)
            {
                return;
            }

            _pointer = eventData.pointerId;
            _input.SetDroneElevation(_ascend, true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pointer == eventData.pointerId)
            {
                OnDisable();
            }
        }

        private void Render()
        {
            var touch = Application.isMobilePlatform || Touchscreen.current != null;
            var visible = touch && _drone.IsPiloted && _input.IsEnabled;
            if (visible)
            {
                var showKey = !Application.isMobilePlatform && Keyboard.current != null;
                var keyPrefix = _ascend ? "Space  " : "Ctrl  ";
                _label.text = showKey ? keyPrefix + _actionLabel : _actionLabel;
            }

            gameObject.SetActive(visible);
        }

        private void OnDisable()
        {
            if (_pointer.HasValue)
            {
                _pointer = null;
                _input.SetDroneElevation(_ascend, false);
            }
        }

        private void OnDestroy()
        {
            if (_drone != null)
            {
                _drone.ContextChanged -= Render;
                _input.Cancelled -= Render;
            }
        }
    }
}
