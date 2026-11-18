using System;

namespace FireSlop.Mission
{
    public sealed class MissionModel
    {
        public MissionModel(string residentId)
        {
            if (string.IsNullOrWhiteSpace(residentId))
            {
                throw new ArgumentException("Mission requires an authored resident ID.", nameof(residentId));
            }

            ResidentId = residentId;
        }

        public string ResidentId { get; }
        public bool IsActive { get; private set; }
        public bool IsWon { get; private set; }
        public double ElapsedSeconds { get; private set; }
        public event Action Won;

        public void Begin()
        {
            if (!IsWon)
            {
                IsActive = true;
            }
        }

        public void Advance(double seconds)
        {
            if (IsActive && seconds > 0d && !double.IsInfinity(seconds))
            {
                ElapsedSeconds += seconds;
            }
        }

        public bool TryRescue(string residentId)
        {
            if (!IsActive || residentId != ResidentId)
            {
                return false;
            }

            IsActive = false;
            IsWon = true;
            Won?.Invoke();
            return true;
        }
    }
}
