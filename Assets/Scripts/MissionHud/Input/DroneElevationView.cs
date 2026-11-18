using FireSlop.Drone;
using FireSlop.Firefighter;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class DroneElevationView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private bool _ascend;

        private DroneModel _drone;
        private MissionInputModel _input;
        private int? _pointer;

        [Inject]
        public void Construct(DroneModel drone, MissionInputModel input)
        {
            _drone = drone;
            _input = input;
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
            gameObject.SetActive(touch && _drone.IsPiloted && _input.IsEnabled);
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
