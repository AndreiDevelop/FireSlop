using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSlop.Dispatch
{
    public sealed class LeaderboardEntryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _rank;
        [SerializeField] private TMP_Text _player;
        [SerializeField] private TMP_Text _score;
        [SerializeField] private Image _avatar;
        [SerializeField] private GameObject _playerHighlight;

        public void Render(int rank, LeaderboardEntry entry, Sprite avatar)
        {
            if (_rank == null || _player == null || _score == null || _avatar == null || _playerHighlight == null)
            {
                throw new InvalidOperationException("Assign the leaderboard rank, player, score, avatar and highlight.");
            }

            _rank.text = CompactNumber.Format(rank);
            _player.text = entry == null ? "—" : entry.Name;
            _score.text = entry == null ? "—" : CompactNumber.Format(entry.Score);
            _avatar.gameObject.SetActive(entry != null);
            _avatar.sprite = avatar;
            _playerHighlight.SetActive(entry != null && entry.IsPlayer);
        }
    }
}
