using System.Collections;
using FireSlop.Firefighter;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class HammerController : MonoBehaviour
    {
        [SerializeField] private HammerSwingView _swing;
        [SerializeField] private InteractionConfigSO _config;

        private Camera _camera;
        private InteractionModel _input;
        private Coroutine _strike;
        private ParticleSystem _brickDust;

        [Inject]
        public void Construct(Camera camera, InteractionModel input)
        {
            _camera = camera;
            _input = input;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void Awake()
        {
            if (_swing == null || _config == null || _config.BrickDust == null || _config.AimCollisionMask.value == 0 ||
                _config.HammerImpactTime >= _swing.Duration)
            {
                Debug.LogError("Hammer requires a swing clip, brick dust, a valid contact time, and a collision mask.", this);
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

            StopSwing();
        }

        private void Subscribe()
        {
            _input.HammerPressed += StartSwing;
            _input.HammerInterrupted += StopSwing;
        }

        private void StartSwing()
        {
            if (_strike == null && _swing.isActiveAndEnabled)
            {
                _strike = StartCoroutine(Swing());
            }
        }

        private void StopSwing()
        {
            if (_strike == null)
            {
                return;
            }

            StopCoroutine(_strike);
            _strike = null;
            _swing.Sample(0f);
        }

        private IEnumerator Swing()
        {
            var elapsed = 0f;
            var struck = false;
            _swing.Sample(0f);
            while (elapsed < _swing.Duration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                _swing.Sample(Mathf.Min(elapsed, _swing.Duration));
                if (!struck && elapsed >= _config.HammerImpactTime)
                {
                    Strike();
                    struck = true;
                }
            }

            _swing.Sample(0f);
            _strike = null;
        }

        private void Strike()
        {
            var view = _camera.transform;
            var ray = new Ray(view.position, view.forward);
            if (!Physics.Raycast(ray, out var hit, _config.HammerReach, _config.AimCollisionMask,
                    QueryTriggerInteraction.Ignore))
            {
                return;
            }

            if (hit.collider.TryGetComponent<WindowSectionView>(out var section))
            {
                section.Window.Hit(section, ray.direction, _config);
                return;
            }

            if (hit.collider.TryGetComponent<BreakableBrickView>(out var brick))
            {
                if (brick.TryBreak(ray.direction, _config))
                {
                    PlayBrickDust(hit);
                }

                return;
            }

            if (hit.collider.TryGetComponent<BreakableFragment>(out var piece))
            {
                var door = piece.GetComponentInParent<DoorView>();
                if (door != null)
                {
                    door.Hit(piece, view.forward, _config);
                    return;
                }

                return;
            }

            var intactDoor = hit.collider.GetComponentInParent<DoorView>();
            if (intactDoor != null)
            {
                intactDoor.HitIntact(hit.collider, ray, _config);
            }
        }

        private void PlayBrickDust(RaycastHit hit)
        {
            if (_brickDust == null)
            {
                _brickDust = Instantiate(_config.BrickDust, transform);
            }

            _brickDust.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
            _brickDust.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            _brickDust.Play(false);
        }
    }
}
