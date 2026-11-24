using System;
using UnityEngine;

namespace FireSlop.Dispatch
{
    [CreateAssetMenu(menuName = "Fire Slop/Daily Reward Settings")]
    public sealed class DailyRewardSettingsSO : ScriptableObject
    {
        [SerializeField] private int[] _currencyRewards;

        public int DayCount => _currencyRewards.Length;

        public int RewardAt(int day)
        {
            return _currencyRewards[day];
        }

        public void Validate()
        {
            if (_currencyRewards == null || _currencyRewards.Length != 7)
            {
                throw new InvalidOperationException("Daily rewards require seven authored awards.");
            }

            foreach (int reward in _currencyRewards)
            {
                if (reward <= 0)
                {
                    throw new InvalidOperationException("Daily reward amounts must be positive.");
                }
            }
        }
    }
}
