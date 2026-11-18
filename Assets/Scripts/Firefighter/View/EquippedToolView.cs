using FireSlop.Interaction;
using FireSlop.MissionHud;
using UnityEngine;
using Zenject;

namespace FireSlop.Firefighter
{
    public sealed class EquippedToolView : MonoBehaviour
    {
        [SerializeField] private GameObject _hose;
        [SerializeField] private GameObject _hammer;

        private ToolSelectionModel _tools;
        private InteractionModel _interaction;
        private Renderer[] _hoseRenderers;
        private Renderer[] _hammerRenderers;

        [Inject]
        public void Construct(ToolSelectionModel tools, InteractionModel interaction)
        {
            _tools = tools;
            _interaction = interaction;
            _hoseRenderers = _hose.GetComponentsInChildren<Renderer>(true);
            _hammerRenderers = _hammer.GetComponentsInChildren<Renderer>(true);
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void OnEnable()
        {
            if (_tools != null)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            if (_tools != null)
            {
                _tools.Changed -= Render;
                _interaction.CarryingChanged -= Render;
                _interaction.ContextChanged -= Render;
            }
        }

        private void Subscribe()
        {
            _tools.Changed += Render;
            _interaction.CarryingChanged += Render;
            _interaction.ContextChanged += Render;
            Render();
        }

        private void Render()
        {
            _hose.SetActive(!_interaction.IsCarryingResident && _tools.Selected == 0);
            _hammer.SetActive(!_interaction.IsCarryingResident && _tools.Selected == 1);
            var hideTools = _interaction.IsPilotingDrone;
            SetRenderersHidden(_hoseRenderers, hideTools);
            SetRenderersHidden(_hammerRenderers, hideTools);
        }

        private static void SetRenderersHidden(Renderer[] renderers, bool hidden)
        {
            foreach (var renderer in renderers)
            {
                renderer.forceRenderingOff = hidden;
            }
        }
    }
}
