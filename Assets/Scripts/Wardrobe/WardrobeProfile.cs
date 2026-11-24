using System;
using System.Collections.Generic;
using UnityEngine;

namespace FireSlop.Wardrobe
{
    public sealed class WardrobeProfile
    {
        private const string SaveKey = "FireSlop.Wardrobe.v1";
        private readonly WardrobeSaveData _data;
        private readonly WardrobeCatalogSO _catalog;

        public WardrobeProfile(WardrobeCatalogSO catalog)
        {
            _catalog = catalog;
            string json = PlayerPrefs.GetString(SaveKey);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    _data = JsonUtility.FromJson<WardrobeSaveData>(json);
                }
                catch (ArgumentException error)
                {
                    Debug.LogWarning(error.Message);
                }
            }

            _data ??= new WardrobeSaveData();
            _data.owned ??= new List<string>();
            _data.equipped ??= new List<string>();
            _data.balance = Mathf.Max(0, _data.balance);
        }

        public int Balance => _data.balance;

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
            if (cost > Balance)
            {
                return false;
            }

            _data.equipped.Clear();
            foreach (WardrobeItem item in selection)
            {
                if (!Owns(item))
                {
                    _data.owned.Add(item.Id);
                }

                _data.equipped.Add(item.Id);
            }

            _data.balance -= (int)cost;
            Persist();
            return true;
        }

        public void CreditRescue()
        {
            _data.balance = (int)Math.Min(int.MaxValue, (long)Balance + _catalog.RescueReward);
            Persist();
        }

        private void Persist()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(_data));
            PlayerPrefs.Save();
        }
    }
}
