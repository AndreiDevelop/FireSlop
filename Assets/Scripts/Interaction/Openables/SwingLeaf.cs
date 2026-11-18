using System;
using UnityEngine;

namespace FireSlop.Interaction
{
    [Serializable]
    internal sealed class SwingLeaf
    {
        [SerializeField] private Transform _hinge;
        [SerializeField] private Vector3 _openEulerOffset;

        private Quaternion _closed;
        private Quaternion _open;

        public void Initialize()
        {
            _closed = _hinge.localRotation;
            _open = _closed * Quaternion.Euler(_openEulerOffset);
        }

        public void SetPose(float progress)
        {
            _hinge.localRotation = Quaternion.Slerp(_closed, _open, progress);
        }
    }
}
