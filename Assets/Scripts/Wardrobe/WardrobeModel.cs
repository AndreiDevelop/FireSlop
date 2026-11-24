using System;
using System.Collections.Generic;

namespace FireSlop.Wardrobe
{
    public sealed class WardrobeModel
    {
        private readonly Dictionary<WardrobeCategory, WardrobeItem> _selection = new Dictionary<WardrobeCategory, WardrobeItem>();
        private readonly List<WardrobeItem> _visible = new List<WardrobeItem>();

        public WardrobeModel(WardrobeCatalogSO catalog, WardrobeProfile profile)
        {
            Catalog = catalog;
            Profile = profile;
            Restore();
        }

        public event Action Changed;
        public WardrobeCatalogSO Catalog { get; }
        public WardrobeProfile Profile { get; }
        public IReadOnlyList<WardrobeItem> Visible => _visible;
        public WardrobeCategory Filter { get; private set; }
        public int Page { get; private set; }
        public bool IsOpen { get; private set; }
        public long Total => Profile.Price(_selection.Values);
        public bool HasUnowned => !AllOwned();

        public bool Selected(WardrobeItem item)
        {
            return _selection.ContainsValue(item);
        }

        public bool Wears(string mesh)
        {
            foreach (WardrobeItem item in _selection.Values)
            {
                if (item.Mesh == mesh)
                {
                    return true;
                }
            }

            return false;
        }

        internal void Show(bool open)
        {
            IsOpen = open;
            Restore();
            FilterBy(null);
        }

        internal void FilterBy(WardrobeCategory category)
        {
            Filter = category;
            Page = 0;
            _visible.Clear();
            if (category != null)
            {
                _visible.AddRange(category.Items);
            }
            else
            {
                // Interleave slots so the first page shows a complete starter outfit.
                int row = 0;
                bool added;
                do
                {
                    added = false;
                    foreach (WardrobeCategory slot in Catalog.Categories)
                    {
                        if (row < slot.Items.Count)
                        {
                            _visible.Add(slot.Items[row]);
                            added = true;
                        }
                    }

                    row++;
                }
                while (added);
            }

            Changed?.Invoke();
        }

        internal void TurnPage(int delta, int capacity)
        {
            Page = Math.Clamp(Page + delta, 0, Math.Max(0, (_visible.Count - 1) / capacity));
            Changed?.Invoke();
        }

        internal void Select(WardrobeItem item)
        {
            foreach (WardrobeCategory slot in Catalog.Categories)
            {
                foreach (WardrobeItem candidate in slot.Items)
                {
                    if (candidate == item)
                    {
                        _selection[slot] = item;
                        Changed?.Invoke();
                        return;
                    }
                }
            }
        }

        internal void Commit()
        {
            bool buying = HasUnowned;
            if (IsOpen && Profile.TryCommit(_selection.Values))
            {
                Show(buying);
            }
        }

        private bool AllOwned()
        {
            foreach (WardrobeItem item in _selection.Values)
            {
                if (!Profile.Owns(item))
                {
                    return false;
                }
            }

            return true;
        }

        private void Restore()
        {
            _selection.Clear();
            foreach (WardrobeCategory slot in Catalog.Categories)
            {
                WardrobeItem item = Profile.Equipped(slot);
                if (item != null)
                {
                    _selection.Add(slot, item);
                }
            }
        }
    }
}
