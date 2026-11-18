using System;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class DoorView : MonoBehaviour, IHitTarget
    {
        [SerializeField] private Transform _leaf;
        [SerializeField] private ClickableObject _clickableObject;
        [SerializeField] private bool _isLocked = true;
        [SerializeField] private Openable _openable;
        [SerializeField] private string _contentId;
        [SerializeField] private Renderer _intactLeaf;
        [SerializeField] private Collider _intactSolid;
        [SerializeField] private BreakableFragment[] _fragments;

        [SerializeField] private AudioSource _doorAudio;
        [SerializeField] private AudioClip _woodBreak;

        private DoorModel _door;

        public Openable Openable => _openable;
        public bool IsBroken => _door.IsBroken;
        public bool IsPassable => _openable.IsOpen || IsBroken;
        public Sprite ActionIcon => _clickableObject != null ? _clickableObject.ActionIcon : null;
        public string ContentId => _contentId;
        public event Action Broken;

        [Inject]
        internal void Construct(DoorModel door)
        {
            _door = door;
        }

        private void Awake()
        {
            if (_isLocked)
            {
                _openable.Lock();
            }
        }

        public bool IsLeafHit(Transform hit)
        {
            return hit.IsChildOf(_leaf);
        }

        public void ApplyHit(Ray ray, RaycastHit hit, InteractionConfigSO config)
        {
            if (hit.collider.TryGetComponent<BreakableFragment>(out var fragment))
            {
                Hit(fragment, ray.direction, config);
                return;
            }

            if (hit.collider != _intactSolid || _door.DamageHits != 0)
            {
                return;
            }

            var struck = BreakableFragment.FindFirstHit(_intactSolid, _fragments, ray, config.HammerReach);
            if (struck != null)
            {
                Hit(struck, ray.direction, config);
            }
        }

        private void Hit(BreakableFragment fragment, Vector3 direction, InteractionConfigSO config)
        {
            if (!IsLeafHit(fragment.transform) || !_door.RegisterHit(config.DoorHitsToBreak))
            {
                return;
            }

            if (_door.DamageHits == 1)
            {
                _intactLeaf.enabled = false;
                foreach (var piece in _fragments)
                {
                    piece.Reveal();
                }
            }

            if (_doorAudio != null && _woodBreak != null)
            {
                _doorAudio.PlayOneShot(_woodBreak);
            }

            fragment.Detach(transform, direction, config);
            if (!_door.IsBroken)
            {
                return;
            }

            foreach (var piece in _fragments)
            {
                piece.Detach(transform, direction, config);
            }

            _leaf.gameObject.SetActive(false);
            Broken?.Invoke();
        }
    }
}
