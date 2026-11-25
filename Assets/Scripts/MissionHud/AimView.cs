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

        private ToolSelectionModel _tools;
        private DroneModel _drone;
        private InteractionModel _interaction;
        private ThrowPreviewModel _preview;

        [Inject]
        public void Construct(DroneModel drone, ThrowPreviewModel preview, InteractionModel interaction, ToolSelectionModel tools)
        {
            if (_body == null)
            {
                throw new InvalidOperationException("Assign the authored crosshair Body in Aim View.");
            }

            _tools = tools;
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
                _tools.LockChanged += Render;
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
                _tools.LockChanged -= Render;
                _preview.VisibilityChanged -= Render;
            }
        }

        private void Render()
        {
            _body.SetActive(!_tools.IsLocked && !_interaction.IsCarrying && !_drone.IsPiloted && !_preview.IsVisible);
        }
    }
}
