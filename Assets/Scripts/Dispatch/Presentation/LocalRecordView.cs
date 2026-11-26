using System;
using FireSlop.Wardrobe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.Dispatch
{
    public sealed class LocalRecordView : MonoBehaviour
    {
        [SerializeField] private GameObject _body;
        [SerializeField] private Button _close;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private LeaderboardEntryView[] _podium;
        [SerializeField] private LeaderboardEntryView[] _rows;
        [SerializeField] private LeaderboardEntryView _current;
        private DispatchModel _model;
        private DispatchController _controller;
        private WardrobeProfile _profile;
        private LeaderboardModel _leaderboard;
        private LeaderboardSettingsSO _settings;

        [Inject]
        public void Construct(DispatchModel model, DispatchController controller, WardrobeProfile profile,
            LeaderboardModel leaderboard, LeaderboardSettingsSO settings)
        {
            if (_body == null || _close == null || _status == null || _current == null
                || _podium == null || _podium.Length != 3 || _rows == null || _rows.Length == 0)
            {
                throw new InvalidOperationException("Assign the leaderboard body, close, status, podium and rows.");
            }

            _model = model;
            _controller = controller;
            _profile = profile;
            _leaderboard = leaderboard;
            _settings = settings;
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
            _leaderboard.Changed += Render;
            _close.onClick.AddListener(_controller.Close);
            Render();
        }

        private void OnDisable()
        {
            if (_model == null)
            {
                return;
            }

            _model.Changed -= Render;
            _leaderboard.Changed -= Render;
            _close.onClick.RemoveListener(_controller.Close);
        }

        private void Render()
        {
            bool open = _model.Panel == DispatchPanel.Record;
            _body.SetActive(open);
            if (!open)
            {
                return;
            }

            _status.text = _profile.LoadError == null ? "LOCAL RANKING • TOTAL MISSION SCORE" : "PROFILE UNAVAILABLE";
            for (int index = 0; index < _podium.Length; index++)
            {
                RenderEntry(_podium[index], index);
            }

            for (int index = 0; index < _rows.Length; index++)
            {
                int entryIndex = _podium.Length + index;
                _rows[index].gameObject.SetActive(entryIndex < _leaderboard.Count);
                if (entryIndex < _leaderboard.Count)
                {
                    RenderEntry(_rows[index], entryIndex);
                }
            }

            _current.Render(_leaderboard.PlayerRank, _leaderboard.Player, _settings.PlayerAvatar);
        }

        private void RenderEntry(LeaderboardEntryView view, int index)
        {
            LeaderboardEntry entry = index < _leaderboard.Count ? _leaderboard.EntryAt(index) : null;
            Sprite avatar = null;
            if (entry != null)
            {
                avatar = entry.IsPlayer ? _settings.PlayerAvatar : _settings.RivalAt(entry.AvatarIndex).Avatar;
            }

            view.Render(index + 1, entry, avatar);
        }
    }
}
