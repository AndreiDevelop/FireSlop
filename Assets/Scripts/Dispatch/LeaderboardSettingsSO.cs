using System;
using UnityEngine;

namespace FireSlop.Dispatch
{
    [CreateAssetMenu(menuName = "Fire Slop/Leaderboard Settings")]
    public sealed class LeaderboardSettingsSO : ScriptableObject
    {
        [SerializeField, Min(1)] private int _scorePerMission;
        [SerializeField] private Sprite _playerAvatar;
        [SerializeField] private LeaderboardRival[] _rivals;

        public int ScorePerMission => _scorePerMission;
        public Sprite PlayerAvatar => _playerAvatar;
        public int RivalCount => _rivals.Length;

        public LeaderboardRival RivalAt(int index)
        {
            return _rivals[index];
        }

        public void Validate()
        {
            if (_scorePerMission <= 0 || _playerAvatar == null || _rivals == null || _rivals.Length > 9)
            {
                throw new InvalidOperationException("Assign a positive mission score, player avatar and up to nine rivals.");
            }

            foreach (LeaderboardRival rival in _rivals)
            {
                if (rival == null || string.IsNullOrWhiteSpace(rival.Name) || rival.Score < 0 || rival.Avatar == null)
                {
                    throw new InvalidOperationException("Each leaderboard rival needs a name, nonnegative score and avatar.");
                }
            }
        }
    }
}
