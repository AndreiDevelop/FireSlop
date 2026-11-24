using System;
using FireSlop.Interaction;
using UnityEngine;
using Zenject;

namespace FireSlop.Cats
{
    public sealed class CatInstaller : MonoInstaller
    {
        [SerializeField] private CatConfigSO _config;

        public override void InstallBindings()
        {
            if (_config == null || !_config.IsValid)
            {
                throw new InvalidOperationException("Assign valid Cat Settings on the Cat prefab installer.");
            }

            Container.Bind<CatConfigSO>().FromInstance(_config);
            Container.Bind<StunModel>().AsSingle();
            Container.Bind<CatModel>().AsSingle();
            Container.Bind<CatPatrolModel>().AsSingle();
        }
    }
}
