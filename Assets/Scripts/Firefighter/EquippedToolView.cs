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

        [Inject]
        public void Construct(ToolSelectionModel tools)
        {
            _tools = tools;
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
            }
        }

        private void Subscribe()
        {
            _tools.Changed += Render;
            Render();
        }

        private void Render()
        {
            _hose.SetActive(_tools.Selected == 0);
            _hammer.SetActive(_tools.Selected == 1);
        }
    }
}
