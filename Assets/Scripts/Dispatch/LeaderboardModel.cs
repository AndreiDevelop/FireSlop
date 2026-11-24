using System;
using System.Collections.Generic;
using FireSlop.Wardrobe;

namespace FireSlop.Dispatch
{
    public sealed class LeaderboardModel : IDisposable
    {
        private readonly LeaderboardSettingsSO _settings;
        private readonly WardrobeProfile _profile;
        private readonly List<LeaderboardEntry> _entries = new List<LeaderboardEntry>();

        public LeaderboardModel(LeaderboardSettingsSO settings, WardrobeProfile profile)
        {
            _settings = settings;
            _profile = profile;
            _profile.Changed += Rebuild;
            Rebuild();
        }

        public event Action Changed;
        public int Count => _entries.Count;
        public int PlayerRank { get; private set; }
        public LeaderboardEntry Player { get; private set; }

        public LeaderboardEntry EntryAt(int index)
        {
            return _entries[index];
        }

        private void Rebuild()
        {
            _entries.Clear();
            Player = new LeaderboardEntry("YOU", _profile.TotalScore, -1);
            _entries.Add(Player);
            for (int index = 0; index < _settings.RivalCount; index++)
            {
                LeaderboardRival rival = _settings.RivalAt(index);
                _entries.Add(new LeaderboardEntry(rival.Name, rival.Score, index));
            }

            _entries.Sort(Compare);
            PlayerRank = _entries.IndexOf(Player) + 1;
            Changed?.Invoke();
        }

        private static int Compare(LeaderboardEntry left, LeaderboardEntry right)
        {
            int scoreOrder = right.Score.CompareTo(left.Score);
            if (scoreOrder != 0)
            {
                return scoreOrder;
            }

            if (left.IsPlayer != right.IsPlayer)
            {
                return left.IsPlayer ? -1 : 1;
            }

            return left.AvatarIndex.CompareTo(right.AvatarIndex);
        }

        public void Dispose()
        {
            _profile.Changed -= Rebuild;
        }
    }
}
