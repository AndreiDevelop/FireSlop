using System;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace FireSlop.Cats
{
    public sealed class CatAnimationView : MonoBehaviour
    {
        private const float MinimumWalkingSpeed = 0.05f;
        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int WalkState = Animator.StringToHash("Walk");
        private static readonly int JumpState = Animator.StringToHash("JumpAttack");
        private static readonly int GrabState = Animator.StringToHash("FaceGrab");

        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private CatCarryable _carryable;
        [SerializeField] private StunReactionView _stunReaction;
        [SerializeField, Min(0f)] private float _transitionSeconds;

        private CatModel _model;
        private CatConfigSO _config;
        private MissionInputModel _input;
        private int _renderedState;

        [Inject]
        public void Construct(CatModel model, CatConfigSO config, MissionInputModel input)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null || _agent == null ||
                _carryable == null || _stunReaction == null)
            {
                throw new InvalidOperationException("Cat Animation View requires its Animator, agent, carryable and stun reaction.");
            }

            _model = model;
            _config = config;
            _input = input;
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_model == null || !isActiveAndEnabled)
            {
                return;
            }

            _model.StateChanged -= Render;
            _model.StateChanged += Render;
            _renderedState = 0;
            Render();
        }

        private void OnDisable()
        {
            if (_model != null)
            {
                _model.StateChanged -= Render;
            }

            if (_stunReaction != null)
            {
                _stunReaction.SetSustained(false);
            }
        }

        private void LateUpdate()
        {
            if (_model != null)
            {
                Render();
            }
        }

        private void Render()
        {
            if (_carryable.IsKnockedDown)
            {
                _stunReaction.SetSustained(false);
                _renderedState = 0;
                return;
            }

            var isStunned = !_carryable.IsCarried && (_model.State == CatState.Settled || _carryable.IsShowingRecovery);
            _stunReaction.SetSustained(isStunned);
            if (_stunReaction.IsShowing)
            {
                _renderedState = 0;
                _animator.speed = _input.IsEnabled ? 1f : 0f;
                return;
            }

            var walkingSpeed = _model.State == CatState.Rescued ? _config.RoamSpeed : 0f;
            if (_model.State == CatState.Idle && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                walkingSpeed = _agent.velocity.magnitude;
            }

            var state = SelectState(walkingSpeed > MinimumWalkingSpeed);
            if (_renderedState != state)
            {
                _animator.CrossFadeInFixedTime(state, _transitionSeconds, 0);
                _renderedState = state;
            }

            var speed = state == WalkState ? walkingSpeed / _config.RoamSpeed : 1f;
            _animator.speed = _input.IsEnabled ? speed : 0f;
        }

        private int SelectState(bool isWalking)
        {
            if (_carryable.IsLanding)
            {
                return JumpState;
            }

            switch (_model.State)
            {
                case CatState.Jumping:
                case CatState.Returning:
                    return JumpState;
                case CatState.Attached:
                    return GrabState;
                case CatState.Settled:
                    return IdleState;
                default:
                    return isWalking ? WalkState : IdleState;
            }
        }
    }
}
