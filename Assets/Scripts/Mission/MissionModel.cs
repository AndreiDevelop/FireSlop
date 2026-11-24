using System;
using System.Collections.Generic;

namespace FireSlop.Mission
{
    public sealed class MissionModel
    {
        private readonly HashSet<string> _residents = new HashSet<string>();
        private readonly HashSet<string> _revealed = new HashSet<string>();
        private readonly HashSet<string> _rescued = new HashSet<string>();

        public MissionModel(params string[] residentIds)
        {
            if (residentIds == null || residentIds.Length == 0)
            {
                throw new ArgumentException("Mission requires authored resident IDs.", nameof(residentIds));
            }

            foreach (var id in residentIds)
            {
                if (string.IsNullOrWhiteSpace(id) || !_residents.Add(id))
                {
                    throw new ArgumentException("Resident IDs must be nonempty and unique.", nameof(residentIds));
                }
            }

            ResidentIds = Array.AsReadOnly((string[])residentIds.Clone());
        }

        public IReadOnlyList<string> ResidentIds { get; }
        public int RequiredRescues => _residents.Count;
        public int RescuedCount => _rescued.Count;
        public bool IsActive { get; private set; }
        public bool IsWon { get; private set; }
        public double ElapsedSeconds { get; private set; }
        public event Action Changed;
        public event Action Won;

        public bool IsResident(string id)
        {
            return id != null && _residents.Contains(id);
        }

        public bool IsRescued(string id)
        {
            return id != null && _rescued.Contains(id);
        }

        public bool IsRevealed(string id)
        {
            return id != null && _revealed.Contains(id);
        }

        public void RevealResident(string id)
        {
            if (IsResident(id) && _revealed.Add(id))
            {
                Changed?.Invoke();
            }
        }

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
            if (!IsActive || !IsResident(residentId) || !_rescued.Add(residentId))
            {
                return false;
            }

            IsWon = RescuedCount == RequiredRescues;
            IsActive = !IsWon;
            Changed?.Invoke();
            if (IsWon)
            {
                Won?.Invoke();
            }

            return true;
        }
    }
}
