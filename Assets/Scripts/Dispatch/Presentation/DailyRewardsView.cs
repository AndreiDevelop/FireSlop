using System;
using FireSlop.Wardrobe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.Dispatch
{
    public sealed class DailyRewardsView : MonoBehaviour
    {
        [SerializeField] private GameObject _body;
        [SerializeField] private Button _close;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private TMP_Text _feedback;
        [SerializeField] private DailyRewardCardView[] _cards;
        [SerializeField] private TMP_Text _featuredAmount;
        [SerializeField] private Button _featuredClaim;
        private DispatchModel _model;
        private DispatchController _controller;
        private WardrobeProfile _profile;
        private DailyRewardSettingsSO _rewards;
        private long _lastSecond;

        [Inject]
        public void Construct(DispatchModel model, DispatchController controller,
            WardrobeProfile profile, DailyRewardSettingsSO rewards)
        {
            if (_body == null || _close == null || _status == null || _feedback == null
                || _cards == null || _cards.Length != rewards.DayCount)
            {
                throw new InvalidOperationException("Assign the Daily Rewards body, labels, close and seven cards.");
            }

            _model = model;
            _controller = controller;
            _profile = profile;
            _rewards = rewards;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void OnEnable()
        {
            if (_model == null)
            {
                return;
            }

            _model.Changed += Render;
            _profile.Changed += Render;
            _close.onClick.AddListener(_controller.Close);
            _featuredClaim.onClick.AddListener(ClaimFeatured);
            for (int index = 0; index < _cards.Length; index++)
            {
                _cards[index].Initialize(index, _controller.Claim);
            }

            Render();
        }

        private void OnDisable()
        {
            if (_model == null)
            {
                return;
            }

            _model.Changed -= Render;
            _profile.Changed -= Render;
            _close.onClick.RemoveListener(_controller.Close);
            _featuredClaim.onClick.RemoveListener(ClaimFeatured);
            foreach (DailyRewardCardView card in _cards)
            {
                card.Cleanup();
            }
        }

        private void ClaimFeatured()
        {
            _controller.Claim(_profile.DailyClaimCount);
        }

        private void Update()
        {
            if (_model == null || _model.Panel != DispatchPanel.DailyRewards)
            {
                return;
            }

            long second = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
            if (second != _lastSecond)
            {
                _lastSecond = second;
                Render();
            }
        }

        private void Render()
        {
            bool open = _model.Panel == DispatchPanel.DailyRewards;
            _body.SetActive(open);
            if (!open)
            {
                return;
            }

            DateTime now = DateTime.UtcNow;
            bool canClaim = _profile.CanClaimDaily(now);
            int featuredDay = Math.Min(_profile.DailyClaimCount, _rewards.DayCount - 1);
            _featuredAmount.text = $"<sprite name=\"Coin\"> {CompactNumber.Format(_rewards.RewardAt(featuredDay))}";
            _featuredClaim.interactable = canClaim && _profile.LoadError == null;
            _feedback.text = _model.Feedback ?? "Missed a day? Your progress is safe.";
            if (_profile.LoadError != null)
            {
                _status.text = "PROFILE UNAVAILABLE";
                _feedback.text = _profile.LoadError;
            }
            else if (_profile.DailyClaimCount == _rewards.DayCount)
            {
                _status.text = "ALL 7 REWARDS COLLECTED!";
            }
            else if (canClaim)
            {
                _status.text = "DAY " + CompactNumber.Format(_profile.DailyClaimCount + 1) + " IS READY TO CLAIM";
            }
            else
            {
                long nextTicks = Math.Min(DateTime.MaxValue.Ticks,
                    _profile.LastDailyClaimUtcTicks + TimeSpan.TicksPerDay);
                TimeSpan remaining = new DateTime(nextTicks, DateTimeKind.Utc) - now;
                _status.text = $"NEXT REWARD IN {(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00} (UTC)";
            }

            for (int index = 0; index < _cards.Length; index++)
            {
                _cards[index].Render(_rewards.RewardAt(index), _profile.DailyClaimCount, canClaim);
            }
        }
    }
}
