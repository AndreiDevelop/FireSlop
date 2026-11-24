using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace FireSlop.Wardrobe
{
    [CreateAssetMenu(menuName = "Fire Slop/Wardrobe Catalog")]
    public sealed class WardrobeCatalogSO : ScriptableObjectInstaller
    {
        [SerializeField] private WardrobeCategory[] _categories;
        [SerializeField, Min(0)] private int _rescueReward;

        public IReadOnlyList<WardrobeCategory> Categories => _categories;
        public int RescueReward => _rescueReward;

        public override void InstallBindings()
        {
            Container.BindInstance(this);
            Container.Bind<WardrobeProfile>().AsSingle();
            Container.Bind<WardrobeModel>().AsSingle();
            Container.Bind<WardrobeController>().AsSingle();
        }
    }
}
