using FireSlop.Firefighter;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class DroneElevationView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private bool _ascend;

        private MissionInputModel _input;
        private int? _pointer;

        [Inject]
        public void Construct(MissionInputModel input)
        {
            _input = input;
            if (isActiveAndEnabled)
            {
                _input.Cancelled += Release;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_input == null || !_input.IsEnabled || _pointer.HasValue ||
                eventData.button != PointerEventData.InputButton.Left)
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
                Release();
            }
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.Cancelled += Release;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.Cancelled -= Release;
            }

            Release();
        }

        private void Release()
        {
            if (_pointer.HasValue)
            {
                _pointer = null;
                _input.SetDroneElevation(_ascend, false);
            }
        }
    }
}
