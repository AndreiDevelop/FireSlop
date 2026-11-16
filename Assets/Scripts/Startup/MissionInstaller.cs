using FireSlop.Firefighter;
using FireSlop.MissionHud;
using UnityEngine;
using Zenject;

namespace FireSlop.Startup
{
    public sealed class MissionInstaller : MonoInstaller
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _entrance;
        [SerializeField] private MovementConfigSO _movement;

        public override void InstallBindings()
        {
            Container.Bind<MovementConfigSO>().FromInstance(_movement);
            Container.Bind<MissionInputModel>().AsSingle();
            Container.Bind<ToolSelectionModel>().AsSingle();
            Container.Bind<QuestTargetModel>().FromInstance(new QuestTargetModel(_entrance.position));
            Container.Bind<Camera>().FromInstance(_camera);
        }
    }
}
