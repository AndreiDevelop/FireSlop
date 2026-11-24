using System;
using System.Collections.Generic;
using FireSlop.Dispatch;
using UnityEngine;

namespace FireSlop.Wardrobe
{
    public sealed class WardrobeProfile
    {
        private const string SaveKey = "FireSlop.Wardrobe.v1";
        private WardrobeSaveData _data;
        private readonly WardrobeCatalogSO _catalog;

        public WardrobeProfile(WardrobeCatalogSO catalog)
        {
            _catalog = catalog;
            string json = PlayerPrefs.GetString(SaveKey);
            if (string.IsNullOrEmpty(json))
            {
                _data = new WardrobeSaveData { balance = catalog.StartingBalance };
            }
            else
            {
                try
                {
                    _data = JsonUtility.FromJson<WardrobeSaveData>(json);
                    if (_data == null)
                    {
                        LoadError = "Saved profile could not be read.";
                    }
                }
                catch (ArgumentException error)
                {
                    LoadError = "Saved profile could not be read.";
                    Debug.LogWarning("Wardrobe profile load failed: " + error.Message);
                }
            }

            _data ??= new WardrobeSaveData();
            _data.owned ??= new List<string>();
            _data.equipped ??= new List<string>();
            _data.balance = Mathf.Max(0, _data.balance);
            bool invalidDaily = _data.dailyClaimCount < 0 || _data.dailyClaimCount > 7
                || _data.lastDailyClaimUtcTicks < 0 || _data.lastDailyClaimUtcTicks > DateTime.MaxValue.Ticks
                || _data.lastDailyClaimUtcTicks % TimeSpan.TicksPerDay != 0
                || (_data.dailyClaimCount == 0) != (_data.lastDailyClaimUtcTicks == 0);
            bool invalidRecord = _data.totalScore < 0;
            if (invalidDaily || invalidRecord)
            {
                LoadError = "Saved profile contains invalid reward or record progress.";
            }
        }

        public event Action Changed;
        public string LoadError { get; }
        public int Balance => _data.balance;
        public int DailyClaimCount => _data.dailyClaimCount;
        public long LastDailyClaimUtcTicks => _data.lastDailyClaimUtcTicks;
        public int TotalScore => _data.totalScore;

        public bool CanClaimDaily(DateTime utcNow)
        {
            return LoadError == null && DailyClaimCount < 7 && utcNow.Date.Ticks > LastDailyClaimUtcTicks;
        }

        public bool TryClaimDaily(DateTime utcNow, DailyRewardSettingsSO rewards)
        {
            rewards.Validate();
            if (!CanClaimDaily(utcNow))
            {
                return false;
            }

            WardrobeSaveData next = Copy();
            next.balance = checked(Balance + rewards.RewardAt(DailyClaimCount));
            next.dailyClaimCount++;
            next.lastDailyClaimUtcTicks = utcNow.Date.Ticks;
            Commit(next);
            return true;
        }

        public bool Owns(WardrobeItem item)
        {
            return item.InitiallyOwned || _data.owned.Contains(item.Id);
        }

        public WardrobeItem Equipped(WardrobeCategory category)
        {
            WardrobeItem fallback = null;
            foreach (WardrobeItem item in category.Items)
            {
                if (Owns(item) && _data.equipped.Contains(item.Id))
                {
                    return item;
                }

                if (fallback == null && item.InitiallyOwned)
                {
                    fallback = item;
                }
            }

            return fallback;
        }

        public long Price(IEnumerable<WardrobeItem> selection)
        {
            long total = 0;
            foreach (WardrobeItem item in selection)
            {
                if (!Owns(item))
                {
                    total += item.Price;
                }
            }

            return total;
        }

        internal bool TryCommit(IEnumerable<WardrobeItem> selection)
        {
            long cost = Price(selection);
            if (LoadError != null || cost > Balance)
            {
                return false;
            }

            WardrobeSaveData next = Copy();
            next.equipped.Clear();
            foreach (WardrobeItem item in selection)
            {
                if (!Owns(item))
                {
                    next.owned.Add(item.Id);
                }

                next.equipped.Add(item.Id);
            }

            next.balance -= (int)cost;
            Commit(next);
            return true;
        }

        public void CreditMissionSuccess(int score)
        {
            if (LoadError != null)
            {
                Debug.LogError("Rescue reward blocked: " + LoadError);
                return;
            }

            if (score <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(score), "Mission score must be positive.");
            }

            WardrobeSaveData next = Copy();
            next.totalScore = (int)Math.Min(int.MaxValue, (long)TotalScore + score);
            next.balance = (int)Math.Min(int.MaxValue, (long)Balance + _catalog.RescueReward);
            Commit(next);
        }

        private WardrobeSaveData Copy()
        {
            return JsonUtility.FromJson<WardrobeSaveData>(JsonUtility.ToJson(_data));
        }

        private void Commit(WardrobeSaveData next)
        {
            string previous = PlayerPrefs.GetString(SaveKey);
            try
            {
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(next));
                PlayerPrefs.Save();
            }
            catch (Exception)
            {
                PlayerPrefs.SetString(SaveKey, previous);
                throw;
            }

            _data = next;
            Changed?.Invoke();
        }
    }
}
