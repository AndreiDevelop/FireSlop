using System;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class ThrowArcView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _arc;

        private readonly Vector3[] _positions = new Vector3[BallisticTrajectory.MaxPoints];
        private readonly BallisticTrajectory _path = new BallisticTrajectory();
        private Transform _player;

        public BallisticTrajectory Path => _path;

        [Inject]
        public void Construct(Camera camera)
        {
            if (_arc == null)
            {
                throw new InvalidOperationException("Assign the authored throw arc renderer.");
            }

            _player = camera.GetComponentInParent<CharacterController>().transform;
        }

        public void Trace(ThrowPoseView pose, Transform ignored = null)
        {
            _path.Trace(pose.EyePosition, pose.LaunchPosition, pose.LaunchVelocity, pose.Radius, _player, ignored);
            _path.CopyPositions(_positions);
            _arc.positionCount = _path.PointCount;
            _arc.SetPositions(_positions);
        }

        public void SetVisible(bool visible)
        {
            _arc.enabled = visible;
        }

        private void OnDisable()
        {
            if (_arc != null)
            {
                _arc.enabled = false;
            }
        }
    }
}
