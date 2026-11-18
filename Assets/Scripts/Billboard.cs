using UnityEngine;
using Zenject;

namespace FireSlop
{
    public sealed class Billboard : MonoBehaviour
    {
        private Transform _cameraTransform;

        [Inject]
        public void Construct(Camera camera)
        {
            _cameraTransform = camera.transform;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null)
            {
                return;
            }

            var direction = _cameraTransform.position - transform.position;
            if (direction == Vector3.zero)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction, _cameraTransform.up);
        }
    }
}
