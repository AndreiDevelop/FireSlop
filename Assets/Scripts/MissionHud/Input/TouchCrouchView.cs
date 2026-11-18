using FireSlop.Firefighter;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class TouchCrouchView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private MissionInputModel _input;
        private int? _pointerId;

        [Inject]
        public void Construct(MissionInputModel input)
        {
            _input = input;
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (_input == null || !_input.IsEnabled || _pointerId.HasValue ||
                data.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _pointerId = data.pointerId;
            _input.SetCrouchHeld(true);
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (_pointerId == data.pointerId)
            {
                Release();
            }
        }

        private void OnDisable()
        {
            Release();
        }

        private void Release()
        {
            if (!_pointerId.HasValue)
            {
                return;
            }

            _pointerId = null;
            _input.SetCrouchHeld(false);
        }
    }
}
