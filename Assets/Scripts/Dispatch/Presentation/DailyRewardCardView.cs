using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSlop.Dispatch
{
    public sealed class DailyRewardCardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _day;
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private TMP_Text _actionLabel;
        [SerializeField] private Button _claim;
        private int _index;
        private Action<int> _onClaim;

        public void Initialize(int index, Action<int> onClaim)
        {
            if (_day == null || _amount == null || _actionLabel == null || _claim == null)
            {
                throw new InvalidOperationException("Assign all Daily Reward Card controls.");
            }

            Cleanup();
            _index = index;
            _onClaim = onClaim;
            _claim.onClick.AddListener(Claim);
        }

        public void Render(int amount, int claimed, bool canClaim)
        {
            bool isClaimed = _index < claimed;
            bool isReady = _index == claimed && canClaim;
            _day.text = "DAY " + CompactNumber.Format(_index + 1);
            _amount.text = $"<sprite name=\"Coin\"> {CompactNumber.Format(amount)}";
            _claim.interactable = isReady;
            _actionLabel.text = "Upcoming";
            if (isClaimed)
            {
                _actionLabel.text = "Claimed";
            }
            else if (isReady)
            {
                _actionLabel.text = "Claim";
            }
        }

        public void Cleanup()
        {
            if (_claim != null)
            {
                _claim.onClick.RemoveListener(Claim);
            }

            _onClaim = null;
        }

        private void Claim()
        {
            _onClaim?.Invoke(_index);
        }
    }
}
