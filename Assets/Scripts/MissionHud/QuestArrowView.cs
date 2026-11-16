using UnityEngine;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class QuestArrowView : MonoBehaviour
    {
        [SerializeField] private RectTransform _arrow;

        private Camera _camera;
        private QuestTargetModel _target;

        [Inject]
        public void Construct(Camera camera, QuestTargetModel target)
        {
            _camera = camera;
            _target = target;
        }

        private void LateUpdate()
        {
            if (_camera == null)
            {
                return;
            }

            var offset = _target.Position - _camera.transform.position;
            var heading = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up);
            var direction = Vector3.ProjectOnPlane(offset, Vector3.up);
            var angle = Vector3.SignedAngle(heading, direction, Vector3.up);
            _arrow.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }
    }
}
