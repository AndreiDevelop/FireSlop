using System;
using UnityEngine;

namespace FireSlop.Wardrobe
{
    [Serializable]
    public sealed class WardrobeFraming
    {
        [SerializeField] private Vector3 _focus;
        [SerializeField, Min(0.05f)] private float _size;
        [SerializeField, Min(0.5f)] private float _distance;

        public Vector3 Focus => _focus;
        public float Size => _size;
        public float Distance => _distance;
    }
}
