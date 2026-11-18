using System.Collections;
using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.MissionHud
{
    public abstract class ContextPromptView : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float _closedMessageSeconds = 1.25f;

        private Coroutine _closedMessage;
        protected InteractionModel Model { get; private set; }

        [Inject]
        public void Construct(InteractionModel model)
        {
            Model = model;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        protected void OnEnable()
        {
            if (Model != null)
            {
                Model.ContextChanged += Render;
                Model.ActionBlocked += ShowClosed;
                Render();
            }
        }

        protected void OnDisable()
        {
            if (Model != null)
            {
                Model.ContextChanged -= Render;
                Model.ActionBlocked -= ShowClosed;
                ClearClosed();
            }
        }

        private void Render()
        {
            if (Model.ContextPrompt.BlockedMessage == null)
            {
                ClearClosed();
            }

            var prompt = Model.ContextPrompt;
            var label = _closedMessage != null ? prompt.BlockedMessage : prompt.Label;
            Display(label ?? string.Empty, _closedMessage != null);
        }

        protected abstract void Display(string action, bool closed);

        private void ShowClosed()
        {
            ClearClosed();
            _closedMessage = StartCoroutine(HideClosed());
            Render();
        }

        private IEnumerator HideClosed()
        {
            yield return new WaitForSecondsRealtime(_closedMessageSeconds);
            _closedMessage = null;
            Render();
        }

        private void ClearClosed()
        {
            if (_closedMessage != null)
            {
                StopCoroutine(_closedMessage);
                _closedMessage = null;
            }
        }
    }
}
