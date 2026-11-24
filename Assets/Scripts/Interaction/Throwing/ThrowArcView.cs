using System;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class ThrowArcView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _arc;
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        private readonly Vector3[] _positions = new Vector3[BallisticTrajectory.MaxPoints];
        private readonly BallisticTrajectory _path = new BallisticTrajectory();
        private readonly object _visibilitySource = new object();
        private Transform _player;
        private ThrowPreviewModel _preview;

        public BallisticTrajectory Path => _path;

        [Inject]
        public void Construct(Camera camera, ThrowPreviewModel preview)
        {
            if (_arc == null)
            {
                throw new InvalidOperationException("Assign the authored throw arc renderer.");
            }

            _player = camera.GetComponentInParent<CharacterController>().transform;
            _preview = preview;
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
            var show = visible && isActiveAndEnabled;
            _arc.enabled = show;
            if (_preview != null)
            {
                _preview.SetVisible(_visibilitySource, show);
            }
        }

        public void SetValidity(bool canThrow)
        {
            var color = canThrow ? _validColor : _invalidColor;
            _arc.startColor = color;
            _arc.endColor = color;
        }

        private void OnDisable()
        {
            if (_arc != null)
            {
                SetVisible(false);
            }
        }
    }
}
