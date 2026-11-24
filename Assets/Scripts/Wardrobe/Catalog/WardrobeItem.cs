using System;
using UnityEngine;

namespace FireSlop.Wardrobe
{
    [Serializable]
    public sealed class WardrobeItem
    {
        [SerializeField] private string _id;
        [SerializeField] private string _mesh;
        [SerializeField] private Sprite _icon;
        [SerializeField, Min(0)] private int _price;
        [SerializeField] private bool _initiallyOwned;

        public string Id => _id;
        public string Mesh => _mesh;
        public Sprite Icon => _icon;
        public int Price => _price;
        public bool InitiallyOwned => _initiallyOwned;
    }
}
