using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSlop.Wardrobe
{
    public sealed class WardrobeCardView : WardrobeView
    {
        [SerializeField] private int _index;
        [SerializeField] private WardrobeGridView _grid;
        [SerializeField] private GameObject _body;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private GameObject _highlight;
        [SerializeField] private GameObject _emptyIcon;

        private WardrobeItem _item;

        protected override void Render()
        {
            int index = Model.Page * _grid.Capacity + _index;
            _item = index < Model.Visible.Count ? Model.Visible[index] : null;
            _body.SetActive(_item != null);
            if (_item == null)
            {
                return;
            }

            _name.text = _item.DisplayName;
            _highlight.SetActive(Model.Selected(_item));
            _icon.sprite = _item.Icon;
            _icon.enabled = _item.Icon != null;
            _emptyIcon.SetActive(_item.Icon == null);
            _label.text = Model.Profile.Owns(_item) ? "Owned" : $"<sprite name=\"Coin\"> {CompactNumber.Format(_item.Price)}";
        }

        public void Select()
        {
            if (_item != null)
            {
                Controller.Select(_item);
            }
        }
    }
}
