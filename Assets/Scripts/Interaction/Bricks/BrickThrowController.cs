using System.Collections.Generic;
using FireSlop.Firefighter;
using FireSlop.MissionHud;
using FireSlop.Tools;
using UnityEngine;
using Zenject;

namespace FireSlop.Interaction
{
    public sealed class BrickThrowController : MonoBehaviour
    {
        [SerializeField] private ToolDefinitionSO _tool;
        [SerializeField] private ThrowArcView _trajectory;

        private readonly Stack<ThrowableBrick> _stored = new();
        private MissionInputModel _input;
        private InteractionModel _interaction;
        private ToolSelectionModel _tools;
        private ThrowPoseView _pose;
        private bool _focused = true;

        internal bool CanCollect => _input.IsEnabled && !_interaction.IsPilotingDrone && !_interaction.IsCarrying;
        private bool CanAim => _stored.Count > 0 && _focused && CanCollect && _tools.SelectedId == _tool.Id;

        [Inject]
        public void Construct(MissionInputModel input, InteractionModel interaction, ToolSelectionModel tools,
            ThrowPoseView pose)
        {
            _input = input;
            _interaction = interaction;
            _tools = tools;
            _pose = pose;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        internal void Collect(ThrowableBrick brick)
        {
            if (!CanCollect || !_tools.TryCollect(_tool))
            {
                return;
            }

            brick.gameObject.SetActive(false);
            _stored.Push(brick);
        }

        private void OnEnable()
        {
            if (_interaction != null)
            {
                _interaction.InventoryThrowRequested -= Throw;
                _interaction.InventoryThrowRequested += Throw;
            }
        }

        private void LateUpdate()
        {
            bool visible = CanAim && _stored.Peek().CanLaunch(_pose);
            _trajectory.SetVisible(visible);
            if (visible)
            {
                _trajectory.Trace(_pose);
            }
        }

        private void Throw(ToolAction action)
        {
            if (action == ToolAction.Brick && CanAim && _stored.Peek().CanLaunch(_pose) && _tools.TryCommitUse(action))
            {
                _stored.Pop().Launch(_pose);
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            _focused = focused;
        }

        private void OnDisable()
        {
            if (_interaction != null)
            {
                _interaction.InventoryThrowRequested -= Throw;
            }

            if (_trajectory != null)
            {
                _trajectory.SetVisible(false);
            }
        }
    }
}
