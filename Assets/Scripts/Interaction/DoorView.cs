using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class DoorView : MonoBehaviour
    {
        [SerializeField] private Transform _leaf;
        [SerializeField] private bool _isLocked = true;
        [SerializeField] private Quaternion _openLocalRotation;

        public DoorModel Door { get; private set; }

        private void Awake()
        {
            Door = new DoorModel(_isLocked);
        }

        public bool IsLeafHit(Transform hit)
        {
            return hit.IsChildOf(_leaf);
        }

        public void Render()
        {
            if (Door.IsOpen)
            {
                _leaf.localRotation = _openLocalRotation;
            }

            _leaf.gameObject.SetActive(!Door.IsBroken);
        }
    }
}
