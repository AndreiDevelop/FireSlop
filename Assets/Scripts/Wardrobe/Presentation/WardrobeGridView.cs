using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSlop.Wardrobe
{
    public sealed class WardrobeGridView : WardrobeView
    {
        [SerializeField] private WardrobeCardView[] _cards;
        [SerializeField] private Button _previous;
        [SerializeField] private Button _next;
        [SerializeField] private TMP_Text _page;
        [SerializeField] private TMP_Text _category;

        public int Capacity => _cards.Length;

        protected override void Render()
        {
            _category.text = Model.Filter?.Name ?? "All items";
            int pages = Mathf.Max(1, (Model.Visible.Count + Capacity - 1) / Capacity);
            _previous.interactable = Model.Page > 0;
            _next.interactable = Model.Page + 1 < pages;
            _page.text = $"{CompactNumber.Format(Model.Page + 1)} / {CompactNumber.Format(pages)}";
        }

        public void Previous()
        {
            Controller.TurnPage(-1, Capacity);
        }

        public void Next()
        {
            Controller.TurnPage(1, Capacity);
        }
    }
}
