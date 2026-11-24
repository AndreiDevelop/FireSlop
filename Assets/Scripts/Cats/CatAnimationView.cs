using System;
using FireSlop.Firefighter;
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
        private static readonly int StunnedState = Animator.StringToHash("Stunned");

        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private CatCarryable _carryable;
        [SerializeField] private GameObject _stunStars;
        [SerializeField, Min(0f)] private float _transitionSeconds;

        private CatModel _model;
        private CatConfigSO _config;
        private MissionInputModel _input;
        private int _renderedState;

        [Inject]
        public void Construct(CatModel model, CatConfigSO config, MissionInputModel input)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null || _agent == null ||
                _carryable == null || _stunStars == null)
            {
                throw new InvalidOperationException("Cat Animation View requires its Animator, agent, carryable and stars.");
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

            if (_stunStars != null)
            {
                _stunStars.SetActive(false);
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
            var isStunned = _model.State == CatState.Settled && !_carryable.IsCarried;
            var walkingSpeed = 0f;
            if (_model.State == CatState.Idle && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                walkingSpeed = _agent.velocity.magnitude;
            }

            var state = SelectState(isStunned, walkingSpeed > MinimumWalkingSpeed);
            if (_renderedState != state)
            {
                _animator.CrossFadeInFixedTime(state, _transitionSeconds, 0);
                _renderedState = state;
            }

            var speed = state == WalkState ? walkingSpeed / _config.RoamSpeed : 1f;
            _animator.speed = _input.IsEnabled ? speed : 0f;
            if (_stunStars.activeSelf != isStunned)
            {
                _stunStars.SetActive(isStunned);
            }
        }

        private int SelectState(bool isStunned, bool isWalking)
        {
            switch (_model.State)
            {
                case CatState.Jumping:
                case CatState.Returning:
                    return JumpState;
                case CatState.Attached:
                    return GrabState;
                case CatState.Settled:
                    return isStunned ? StunnedState : IdleState;
                default:
                    return isWalking ? WalkState : IdleState;
            }
        }
    }
}
