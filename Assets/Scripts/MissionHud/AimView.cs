using System;
using FireSlop.Drone;
using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class AimView : MonoBehaviour
    {
        [SerializeField] private GameObject _body;

        private DroneModel _drone;
        private InteractionModel _interaction;
        private ThrowPreviewModel _preview;

        [Inject]
        public void Construct(DroneModel drone, ThrowPreviewModel preview, InteractionModel interaction)
        {
            if (_body == null)
            {
                throw new InvalidOperationException("Assign the authored crosshair Body in Aim View.");
            }

            _drone = drone;
            _interaction = interaction;
            _preview = preview;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void OnEnable()
        {
            if (_preview != null)
            {
                _drone.ContextChanged += Render;
                _interaction.CarryingChanged += Render;
                _preview.VisibilityChanged += Render;
                Render();
            }
        }

        private void OnDisable()
        {
            if (_preview != null)
            {
                _drone.ContextChanged -= Render;
                _interaction.CarryingChanged -= Render;
                _preview.VisibilityChanged -= Render;
            }
        }

        private void Render()
        {
            _body.SetActive(!_interaction.IsCarrying && !_drone.IsPiloted && !_preview.IsVisible);
        }
    }
}
