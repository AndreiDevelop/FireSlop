using System;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class BreakableWallView : MonoBehaviour
    {
        [SerializeField] private string _contentId;

        [SerializeField] private AudioSource _breakAudio;
        [SerializeField] private BreakableBrickView[] _crackCandidates = Array.Empty<BreakableBrickView>();
        [SerializeField] private Material _crackInterior;

        private BreakableBrickView[] _bricks;
        private BreakableWallModel _model;
        private BreakableWallController _controller;

        public string ContentId => _contentId;

        [Inject]
        internal void Construct(BreakableWallModel model, BreakableWallController controller)
        {
            _model = model;
            _controller = controller;
        }

        private void Awake()
        {
            _bricks = GetComponentsInChildren<BreakableBrickView>();
            if (_controller == null || _crackInterior == null || !_controller.SelectCrackedBricks(_crackCandidates.Length))
            {
                Debug.LogError("A breakable wall requires its wall installer, crack material and at least two crack candidates.", this);
                return;
            }

            for (var index = 0; index < _crackCandidates.Length; index++)
            {
                if (_model.IsCracked(index))
                {
                    _crackCandidates[index].ShowCrack(_crackInterior);
                }
            }
        }

        internal void ApplyBrickHit(BreakableBrickView brick, Vector3 direction, InteractionConfigSO config)
        {
            var candidateIndex = Array.IndexOf(_crackCandidates, brick);
            _controller.ApplyHit(this, brick, candidateIndex, direction, config);
        }

        internal void Collapse(Vector3 direction, InteractionConfigSO config)
        {
            // Clear the wall before the debris calculates its travel and landing points.
            foreach (var brick in _bricks)
            {
                brick.DisableCollision();
            }

            foreach (var brick in _bricks)
            {
                if (brick.gameObject.activeInHierarchy)
                {
                    brick.ReleaseDebris(direction, config);
                }
            }
        }

        internal void PlayBrickBreak()
        {
            if (_breakAudio != null && _breakAudio.clip != null)
            {
                _breakAudio.Play();
            }
        }

        internal float GetNextSillRise(float feetY, float previousRise, float clearance)
        {
            var nextRise = float.PositiveInfinity;
            foreach (var brick in _bricks)
            {
                if (!brick.IsSolid)
                {
                    continue;
                }

                var rise = brick.Top - feetY + clearance;
                if (rise > previousRise)
                {
                    nextRise = Mathf.Min(nextRise, rise);
                }
            }

            return nextRise;
        }
    }
}
