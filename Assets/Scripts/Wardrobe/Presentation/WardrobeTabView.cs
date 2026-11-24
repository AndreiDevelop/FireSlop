using UnityEngine;
using UnityEngine.UI;

namespace FireSlop.Wardrobe
{
    public sealed class WardrobeTabView : WardrobeView
    {
        [SerializeField] private int _index;
        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _highlight;

        private WardrobeCategory Category => _index < 0 ? null : Model.Catalog.Categories[_index];

        protected override void Render()
        {
            _highlight.SetActive(Model.Filter == Category);
            if (_icon != null && Category != null)
            {
                _icon.sprite = Category.Icon;
            }
        }

        public void Select()
        {
            Controller.Filter(Category);
        }
    }
}
