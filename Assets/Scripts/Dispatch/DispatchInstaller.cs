using System;
using UnityEngine;
using Zenject;

namespace FireSlop.Dispatch
{
    public sealed class DispatchInstaller : MonoInstaller
    {
        [SerializeField] private DailyRewardSettingsSO _rewards;

        public override void InstallBindings()
        {
            if (_rewards == null)
            {
                throw new InvalidOperationException("Assign Daily Rewards in the Intro Dispatch Installer.");
            }

            _rewards.Validate();
            Container.BindInstance(_rewards);
            Container.Bind<DispatchModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardModel>().AsSingle();
            Container.Bind<DispatchController>().AsSingle();
        }
    }
}
