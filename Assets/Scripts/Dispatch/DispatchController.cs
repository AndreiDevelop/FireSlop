using System;
using FireSlop.Wardrobe;
using UnityEngine;

namespace FireSlop.Dispatch
{
    public sealed class DispatchController
    {
        private readonly DispatchModel _model;
        private readonly WardrobeModel _wardrobe;
        private readonly WardrobeProfile _profile;
        private readonly DailyRewardSettingsSO _rewards;

        public DispatchController(DispatchModel model, WardrobeModel wardrobe,
            WardrobeProfile profile, DailyRewardSettingsSO rewards)
        {
            _model = model;
            _wardrobe = wardrobe;
            _profile = profile;
            _rewards = rewards;
        }

        public void OpenDaily()
        {
            if (!_wardrobe.IsOpen)
            {
                _model.Show(DispatchPanel.DailyRewards);
            }
        }

        public void OpenRecord()
        {
            if (!_wardrobe.IsOpen)
            {
                _model.Show(DispatchPanel.Record);
            }
        }

        public void Close()
        {
            _model.Show(DispatchPanel.None);
        }

        public void Claim(int day)
        {
            if (_model.Panel != DispatchPanel.DailyRewards || _wardrobe.IsOpen
                || day != _profile.DailyClaimCount || day < 0 || day >= _rewards.DayCount)
            {
                return;
            }

            try
            {
                int amount = _rewards.RewardAt(day);
                if (_profile.TryClaimDaily(DateTime.UtcNow, _rewards))
                {
                    _model.Report($"<sprite name=\"Coin\"> {CompactNumber.Format(amount)} RECEIVED!");
                }
            }
            catch (Exception error)
            {
                _model.Report("Could not save reward. Please try again.");
                Debug.LogError("Daily reward claim failed: " + error.Message);
            }
        }
    }
}
