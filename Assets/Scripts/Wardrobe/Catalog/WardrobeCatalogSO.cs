using System;
using System.Collections.Generic;
using FireSlop.Dispatch;
using UnityEngine;
using Zenject;

namespace FireSlop.Wardrobe
{
    [CreateAssetMenu(menuName = "Fire Slop/Wardrobe Catalog")]
    public sealed class WardrobeCatalogSO : ScriptableObjectInstaller
    {
        [SerializeField] private WardrobeCategory[] _categories;
        [SerializeField] private LeaderboardSettingsSO _leaderboard;
        [SerializeField, Min(0)] private int _rescueReward;

        [SerializeField] private WardrobeFraming _fullBody;
        [SerializeField, Min(0f)] private float _cameraDuration;

        public WardrobeFraming FullBody => _fullBody;
        public float CameraDuration => _cameraDuration;
        public IReadOnlyList<WardrobeCategory> Categories => _categories;
        public int RescueReward => _rescueReward;

        public override void InstallBindings()
        {
            if (_leaderboard == null)
            {
                throw new InvalidOperationException("Assign Leaderboard Settings in the Wardrobe Catalog installer.");
            }

            _leaderboard.Validate();
            Container.BindInstance(_leaderboard);
            Container.BindInstance(this);
            Container.Bind<WardrobeProfile>().AsSingle();
            Container.Bind<WardrobeModel>().AsSingle();
            Container.Bind<WardrobeController>().AsSingle();
        }
    }
}
