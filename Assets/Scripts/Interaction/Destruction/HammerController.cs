using System;
using FireSlop.Firefighter;
using FireSlop.MissionHud;
using FireSlop.Tools;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class HammerController : MonoBehaviour
    {
        [SerializeField] private HammerSwingView _swing;
        [SerializeField] private InteractionConfigSO _config;

        private RaycastHit[] _hits = new RaycastHit[16];
        private Camera _camera;
        private InteractionModel _input;
        private ToolSelectionModel _tools;

        [Inject]
        public void Construct(Camera camera, InteractionModel input, ToolSelectionModel tools)
        {
            _camera = camera;
            _input = input;
            _tools = tools;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void Awake()
        {
            if (_swing == null || _config == null || _config.AimCollisionMask.value == 0)
            {
                Debug.LogError("Hammer requires a swing view, interaction settings, and a collision mask.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.HammerPressed -= StartSwing;
                _input.HammerInterrupted -= StopSwing;
            }

            if (_swing != null)
            {
                _swing.Impact -= Strike;
                _swing.StopSwing();
            }
        }

        private void Subscribe()
        {
            _input.HammerPressed += StartSwing;
            _input.HammerInterrupted += StopSwing;
            _swing.Impact += Strike;
        }

        private void StartSwing()
        {
            if (_swing.CanSwing && _tools.TryCommitUse(ToolAction.Hammer))
            {
                _swing.PlaySwing();
            }
        }

        private void StopSwing()
        {
            _swing.StopSwing();
        }

        private void Strike()
        {
            var view = _camera.transform;
            var ray = new Ray(view.position, view.forward);
            int count = Physics.RaycastNonAlloc(ray, _hits, _config.HammerReach, _config.AimCollisionMask,
                QueryTriggerInteraction.Collide);
            while (count == _hits.Length)
            {
                Array.Resize(ref _hits, _hits.Length * 2);
                count = Physics.RaycastNonAlloc(ray, _hits, _config.HammerReach, _config.AimCollisionMask,
                    QueryTriggerInteraction.Collide);
            }

            float nearest = float.PositiveInfinity;
            RaycastHit contact = default;
            IHitTarget target = null;
            for (int index = 0; index < count; index++)
            {
                RaycastHit hit = _hits[index];
                if (hit.distance >= nearest)
                {
                    continue;
                }

                var candidate = hit.collider.GetComponentInParent<IHitTarget>();
                if (hit.collider.isTrigger && candidate?.AcceptsTriggerHits != true)
                {
                    continue;
                }

                nearest = hit.distance;
                contact = hit;
                target = candidate;
            }

            target?.ApplyHit(ray, contact, _config);
        }
    }
}
