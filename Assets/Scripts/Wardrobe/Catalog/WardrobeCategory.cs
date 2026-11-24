using System;
using System.Collections.Generic;
using UnityEngine;

namespace FireSlop.Wardrobe
{
    [Serializable]
    public sealed class WardrobeCategory
    {
        [SerializeField] private string _name;
        [SerializeField] private Sprite _icon;
        [SerializeField] private WardrobeItem[] _items;

        public string Name => _name;
        public Sprite Icon => _icon;
        public IReadOnlyList<WardrobeItem> Items => _items;
    }
}
