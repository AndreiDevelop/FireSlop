using FireSlop.Firefighter;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class TouchLookView : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MissionInputModel _input;
        private int? _pointer;

        [Inject]
        public void Construct(MissionInputModel input)
        {
            _input = input;
            _input.Cancelled += ClearPointer;
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (data is ExtendedPointerEventData { pointerType: UIPointerType.Touch } && !_pointer.HasValue)
            {
                _pointer = data.pointerId;
                data.useDragThreshold = false;
            }
        }

        public void OnDrag(PointerEventData data)
        {
            if (_pointer == data.pointerId)
            {
                _input.AddLook(data.delta);
            }
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (_pointer == data.pointerId)
            {
                ClearPointer();
            }
        }

        private void OnDisable()
        {
            ClearPointer();
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.Cancelled -= ClearPointer;
            }
        }

        private void ClearPointer()
        {
            _pointer = null;
        }
    }
}
