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
                Model.ClosedDoor += ShowClosed;
                Render();
            }
        }

        protected void OnDisable()
        {
            if (Model != null)
            {
                Model.ContextChanged -= Render;
                Model.ClosedDoor -= ShowClosed;
                ClearClosed();
            }
        }

        private void Render()
        {
            if (!Model.CanInteractWithDoor || Model.HasDroneAction)
            {
                ClearClosed();
            }

            var action = GetActionLabel();
            Display(_closedMessage != null ? "Closed" : action, _closedMessage != null);
        }

        protected abstract void Display(string action, bool closed);

        private string GetActionLabel()
        {
            if (Model.HasDroneAction)
            {
                return Model.IsPilotingDrone ? "Exit flight" : "Use drone";
            }

            if (Model.CanInteractWithDoor)
            {
                return Model.ShouldCloseDoor ? "Close door" : "Open door";
            }

            return Model.RescueAction switch
            {
                RescueContext.OpenCabinet => "Open cabinet",
                RescueContext.CloseCabinet => "Close cabinet",
                RescueContext.PickUp => "Pick up",
                RescueContext.PutDown => "Put down",
                _ => string.Empty
            };
        }

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
