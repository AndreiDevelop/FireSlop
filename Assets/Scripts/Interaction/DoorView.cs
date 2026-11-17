using System;
using System.Collections;
using UnityEngine;

namespace FireSlop.Interaction
{
    public sealed class DoorView : MonoBehaviour
    {
        [SerializeField] private Transform _leaf;
        [SerializeField] private bool _isLocked = true;
        [SerializeField] private Quaternion _openLocalRotation;
        [SerializeField] private string _contentId;
        [SerializeField] private Renderer _intactLeaf;
        [SerializeField] private Collider _intactSolid;
        [SerializeField] private BreakableFragment[] _fragments;

        private Quaternion _closedLocalRotation;
        private Coroutine _swing;
        private float _openness;

        public DoorModel Door { get; private set; }
        public string ContentId => _contentId;
        public event Action Broken;

        private void Awake()
        {
            Door = new DoorModel(_isLocked);
            _closedLocalRotation = _leaf.localRotation;
        }

        private void OnDisable()
        {
            if (_swing == null)
            {
                return;
            }

            StopSwing();
            SetOpenness(Door.IsOpen ? 1f : 0f);
        }

        public bool IsLeafHit(Transform hit)
        {
            return hit.IsChildOf(_leaf);
        }

        internal void HitIntact(Collider hit, Ray ray, InteractionConfigSO config)
        {
            if (hit != _intactSolid || Door.DamageHits != 0)
            {
                return;
            }

            _intactSolid.enabled = false;
            foreach (var piece in _fragments)
            {
                piece.Solid.enabled = true;
            }

            Physics.SyncTransforms();
            BreakableFragment struck = null;
            var distance = config.HammerReach;
            foreach (var piece in _fragments)
            {
                if (piece.Solid.Raycast(ray, out var pieceHit, distance))
                {
                    struck = piece;
                    distance = pieceHit.distance;
                }
            }

            if (struck != null)
            {
                Hit(struck, ray.direction, config);
                return;
            }

            _intactSolid.enabled = true;
            foreach (var piece in _fragments)
            {
                piece.Solid.enabled = false;
            }
        }

        internal void Hit(BreakableFragment fragment, Vector3 direction, InteractionConfigSO config)
        {
            if (!IsLeafHit(fragment.transform) || !Door.RegisterHit(config.DoorHitsToBreak))
            {
                return;
            }

            if (Door.DamageHits == 1)
            {
                _intactLeaf.enabled = false;
                foreach (var piece in _fragments)
                {
                    piece.Reveal();
                }
            }

            fragment.Detach(transform, direction, config);
            if (!Door.IsBroken)
            {
                return;
            }

            foreach (var piece in _fragments)
            {
                piece.Detach(transform, direction, config);
            }

            StopSwing();
            _leaf.gameObject.SetActive(false);
            Broken?.Invoke();
        }

        public void Render(float swingDuration)
        {
            StopSwing();
            if (Door.IsBroken)
            {
                _leaf.gameObject.SetActive(false);
                return;
            }

            var target = Door.IsOpen ? 1f : 0f;
            if (_openness == target || !isActiveAndEnabled)
            {
                SetOpenness(target);
                return;
            }

            _swing = StartCoroutine(SwingTo(target, swingDuration));
        }

        private IEnumerator SwingTo(float target, float duration)
        {
            while (_openness != target)
            {
                yield return null;
                SetOpenness(Mathf.MoveTowards(_openness, target, Time.deltaTime / duration));
            }

            _swing = null;
        }

        private void SetOpenness(float openness)
        {
            _openness = openness;
            var easedOpenness = Mathf.SmoothStep(0f, 1f, openness);
            _leaf.localRotation = Quaternion.Slerp(_closedLocalRotation, _openLocalRotation, easedOpenness);
        }

        private void StopSwing()
        {
            if (_swing == null)
            {
                return;
            }

            StopCoroutine(_swing);
            _swing = null;
        }
    }
}
