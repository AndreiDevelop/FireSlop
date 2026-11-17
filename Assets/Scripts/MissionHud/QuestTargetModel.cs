using System;
using FireSlop.Interaction;
using FireSlop.Mission;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class QuestTargetModel : IDisposable
    {
        private readonly InteractionModel _interaction;
        private readonly MissionModel _mission;

        public QuestTargetModel(InteractionModel interaction, [InjectOptional] MissionModel mission)
        {
            _interaction = interaction;
            _mission = mission;
            if (_mission != null)
            {
                _interaction.CabinetRevealed += NotifyChanged;
                _interaction.CarryingChanged += NotifyChanged;
                _mission.Won += NotifyChanged;
            }
            else
            {
                _interaction.ContextChanged += NotifyChanged;
            }
        }

        public QuestStep Step
        {
            get
            {
                if (_mission == null)
                {
                    return _interaction.IsEntrancePassable ? QuestStep.RescuePrototypeResident : QuestStep.ReachEntrance;
                }

                if (_mission.IsWon)
                {
                    return QuestStep.Complete;
                }

                if (_interaction.IsCarryingResident)
                {
                    return QuestStep.DeliverResident;
                }

                return _interaction.HasRevealedResident ? QuestStep.PickUpResident : QuestStep.FindResident;
            }
        }

        public event Action Changed;

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }

        public void Dispose()
        {
            if (_mission == null)
            {
                _interaction.ContextChanged -= NotifyChanged;
                return;
            }

            _interaction.CabinetRevealed -= NotifyChanged;
            _interaction.CarryingChanged -= NotifyChanged;
            _mission.Won -= NotifyChanged;
        }
    }
}
