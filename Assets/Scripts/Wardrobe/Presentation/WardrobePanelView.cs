using UnityEngine;

namespace FireSlop.Wardrobe
{
    public sealed class WardrobePanelView : WardrobeView
    {
        [SerializeField] private GameObject _body;
        [SerializeField] private bool _showInEditor;

        protected override void Render()
        {
            _body.SetActive(Model.IsOpen == _showInEditor);
        }

        public void Open()
        {
            Controller.Open();
        }

        public void Back()
        {
            Controller.Cancel();
        }
    }
}
