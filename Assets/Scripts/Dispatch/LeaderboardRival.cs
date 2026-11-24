using System;
using UnityEngine;

namespace FireSlop.Dispatch
{
    [Serializable]
    public sealed class LeaderboardRival
    {
        [SerializeField] private string _name;
        [SerializeField, Min(0)] private int _score;
        [SerializeField] private Sprite _avatar;

        public string Name => _name;
        public int Score => _score;
        public Sprite Avatar => _avatar;
    }
}
