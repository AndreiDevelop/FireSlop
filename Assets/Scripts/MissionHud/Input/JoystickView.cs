using FireSlop.Firefighter;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class JoystickView : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _pad;
        [SerializeField] private RectTransform _handle;

        private MissionInputModel _input;
        private int? _pointer;

        private MissionHudPlatform _platform;

        [Inject]
        public void Construct(MissionInputModel input, MissionHudPlatform platform)
        {
            _input = input;
            _input.Cancelled += ResetPointer;
            _platform = platform;
            _platform.Changed += RenderVisibility;
            RenderVisibility();
        }

        private void RenderVisibility()
        {
            gameObject.SetActive(_platform.UsesTouchControls);
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (_pointer.HasValue || data.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _pointer = data.pointerId;
            data.useDragThreshold = false;
            OnDrag(data);
        }

        public void OnDrag(PointerEventData data)
        {
            if (_pointer != data.pointerId ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(_pad, data.position, data.pressEventCamera,
                    out var local))
            {
                return;
            }

            var radius = (_pad.rect.width - _handle.rect.width) * 0.5f;
            var direction = Vector2.ClampMagnitude(local / radius, 1f);
            _handle.anchoredPosition = direction * radius;
            _input.SetMovement(direction);
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (_pointer == data.pointerId)
            {
                ResetPointer();
            }
        }

        private void OnDisable()
        {
            ResetPointer();
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.Cancelled -= ResetPointer;
                _platform.Changed -= RenderVisibility;
            }
        }

        private void ResetPointer()
        {
            _pointer = null;
            _input?.SetMovement(Vector2.zero);
            _handle.anchoredPosition = Vector2.zero;
        }
    }
}
