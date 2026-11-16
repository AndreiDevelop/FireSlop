using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class ToolPanelView : MonoBehaviour
    {
        [SerializeField] private Button _hose;
        [SerializeField] private Button _hammer;
        [SerializeField] private GameObject _hoseSelection;
        [SerializeField] private GameObject _hammerSelection;

        private ToolSelectionModel _model;

        [Inject]
        public void Construct(ToolSelectionModel model)
        {
            _model = model;
            if (isActiveAndEnabled)
            {
                Bind();
            }
        }

        private void OnEnable()
        {
            if (_model != null)
            {
                Bind();
            }
        }

        private void OnDisable()
        {
            if (_model == null)
            {
                return;
            }

            _hose.onClick.RemoveListener(SelectHose);
            _hammer.onClick.RemoveListener(SelectHammer);
            _model.Changed -= Render;
        }

        private void Bind()
        {
            _hose.onClick.AddListener(SelectHose);
            _hammer.onClick.AddListener(SelectHammer);
            _model.Changed += Render;
            Render();
        }

        private void SelectHose()
        {
            _model.Select(0);
        }

        private void SelectHammer()
        {
            _model.Select(1);
        }

        private void Render()
        {
            _hoseSelection.SetActive(_model.Selected == 0);
            _hammerSelection.SetActive(_model.Selected == 1);
        }
    }
}
