using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.Dispatch
{
    public sealed class DispatchNavigationView : MonoBehaviour
    {
        [SerializeField] private Button _daily;
        [SerializeField] private Button _record;
        private DispatchController _controller;
        private DispatchModel _model;

        [Inject]
        public void Construct(DispatchModel model, DispatchController controller)
        {
            if (_daily == null || _record == null)
            {
                throw new InvalidOperationException("Assign the Dispatch Rewards and Record buttons.");
            }

            _controller = controller;
            _model = model;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void OnEnable()
        {
            if (_controller == null)
            {
                return;
            }

            _model.Changed += Render;
            _daily.onClick.AddListener(_controller.OpenDaily);
            _record.onClick.AddListener(_controller.OpenRecord);
            Render();
        }

        private void Update()
        {
            if (_model != null && _model.Panel != DispatchPanel.None
                && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _controller.Close();
            }
        }

        private void Render()
        {
            bool available = _model.Panel == DispatchPanel.None;
            _daily.interactable = available;
            _record.interactable = available;
        }

        private void OnDisable()
        {
            if (_controller == null)
            {
                return;
            }

            _model.Changed -= Render;
            _daily.onClick.RemoveListener(_controller.OpenDaily);
            _record.onClick.RemoveListener(_controller.OpenRecord);
        }
    }
}
