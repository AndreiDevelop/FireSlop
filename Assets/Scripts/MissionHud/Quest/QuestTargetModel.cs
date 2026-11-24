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
                _mission.Changed += NotifyChanged;
                _interaction.CarryingChanged += NotifyChanged;
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

                if (_mission.IsResident(_interaction.CarriedContentId))
                {
                    return QuestStep.DeliverResident;
                }

                return _mission.IsRevealed(TargetResidentId) ? QuestStep.PickUpResident : QuestStep.FindResident;
            }
        }

        public string TargetResidentId
        {
            get
            {
                if (_mission == null)
                {
                    return null;
                }

                string hidden = null;
                foreach (var id in _mission.ResidentIds)
                {
                    if (_mission.IsRescued(id))
                    {
                        continue;
                    }

                    if (_mission.IsRevealed(id))
                    {
                        return id;
                    }

                    if (hidden == null)
                    {
                        hidden = id;
                    }
                }

                return hidden;
            }
        }

        public string RescueProgress => _mission != null ? $"{CompactNumber.Format(_mission.RescuedCount)}/{CompactNumber.Format(_mission.RequiredRescues)}" : null;

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

            _mission.Changed -= NotifyChanged;
            _interaction.CarryingChanged -= NotifyChanged;
        }
    }
}
