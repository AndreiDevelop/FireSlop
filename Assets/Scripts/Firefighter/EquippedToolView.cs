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

        [Inject]
        public void Construct(ToolSelectionModel tools, InteractionModel interaction)
        {
            _tools = tools;
            _interaction = interaction;
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
            }
        }

        private void Subscribe()
        {
            _tools.Changed += Render;
            _interaction.CarryingChanged += Render;
            Render();
        }

        private void Render()
        {
            _hose.SetActive(!_interaction.IsCarryingResident && _tools.Selected == 0);
            _hammer.SetActive(!_interaction.IsCarryingResident && _tools.Selected == 1);
        }
    }
}
