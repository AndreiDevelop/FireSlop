using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class ResidentCarryRagdoll : MonoBehaviour
    {
        [SerializeField] private Rigidbody _anchor;
        [SerializeField] private Rigidbody[] _limbs;

        private Transform _home;
        private Transform _shoulder;
        private Vector3[] _restPositions;
        private Quaternion[] _restRotations;
        private Vector3 _anchorPosition;
        private Quaternion _anchorRotation;

        public void Initialize()
        {
            _home = transform.parent;
            _anchorPosition = _anchor.transform.localPosition;
            _anchorRotation = _anchor.transform.localRotation;
            _restPositions = new Vector3[_limbs.Length];
            _restRotations = new Quaternion[_limbs.Length];
            for (var index = 0; index < _limbs.Length; index++)
            {
                _restPositions[index] = _limbs[index].transform.localPosition;
                _restRotations[index] = _limbs[index].transform.localRotation;
            }
        }

        public void HangFrom(Transform shoulder)
        {
            _shoulder = shoulder;
            transform.SetParent(null, false);
            transform.SetPositionAndRotation(shoulder.position, shoulder.rotation);
            _anchor.transform.SetLocalPositionAndRotation(_anchorPosition, _anchorRotation);
            for (var index = 0; index < _limbs.Length; index++)
            {
                _limbs[index].transform.SetLocalPositionAndRotation(_restPositions[index], _restRotations[index]);
            }

            gameObject.SetActive(true);
        }

        public void Stow()
        {
            _shoulder = null;
            gameObject.SetActive(false);
            transform.SetParent(_home, false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void FixedUpdate()
        {
            if (_shoulder == null)
            {
                return;
            }

            _anchor.MovePosition(_shoulder.TransformPoint(_anchorPosition));
            _anchor.MoveRotation(_shoulder.rotation * _anchorRotation);
        }
    }
}
