using FireSlop.Firefighter;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class StunReactionView : MonoBehaviour, IHitTarget
    {
        private static readonly int Stunned = Animator.StringToHash("Base Layer.Stunned");
        private static readonly int Idle = Animator.StringToHash("Base Layer.Idle");

        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _stars;

        private HammerStunModel _model;
        private MissionInputModel _input;
        private IStunEligibility _eligibility;
        private bool _sustained;
        private bool _restoreIdle;

        public bool AcceptsTriggerHits => true;
        public bool IsShowing { get; private set; }

        [Inject]
        public void Construct(HammerStunModel model, MissionInputModel input)
        {
            _model = model;
            _input = input;
            _eligibility = GetComponent<IStunEligibility>();
        }

        public void ApplyHit(Ray ray, RaycastHit hit, InteractionConfigSO config)
        {
            if (_model == null || !_input.IsEnabled || !CanReact())
            {
                return;
            }

            _model.Restart(config.HammerStunSeconds);
            Render(true);
        }

        public void SetSustained(bool sustained)
        {
            _sustained = sustained;
            Refresh();
        }

        private bool CanReact()
        {
            return isActiveAndEnabled && _animator.isActiveAndEnabled &&
                (_eligibility == null || _eligibility.CanBeStunned);
        }

        private void Update()
        {
            if (_restoreIdle && _animator.isActiveAndEnabled)
            {
                _animator.Play(Idle, 0, 0f);
                _restoreIdle = false;
            }

            if (_model == null || !_model.IsActive)
            {
                return;
            }

            if (!CanReact())
            {
                _model.Clear();
            }
            else if (_input.IsEnabled)
            {
                _model.Advance(Time.deltaTime);
            }

            Refresh();
        }

        private void Refresh()
        {
            Render((_sustained || (_model != null && _model.IsActive)) && CanReact());
        }

        private void Render(bool show)
        {
            if (IsShowing == show)
            {
                return;
            }

            IsShowing = show;
            _stars.SetActive(show);
            _restoreIdle = !show && !_animator.isActiveAndEnabled;
            if (_animator.isActiveAndEnabled)
            {
                _animator.CrossFadeInFixedTime(show ? Stunned : Idle, 0.08f, 0);
            }
        }

        private void OnDisable()
        {
            _model?.Clear();
            _sustained = false;
            Render(false);
        }
    }
}
