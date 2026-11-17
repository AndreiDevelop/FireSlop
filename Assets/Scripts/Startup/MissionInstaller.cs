using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.MissionHud;
using FireSlop.Mission;
using UnityEngine;
using Zenject;

namespace FireSlop.Startup
{
    public sealed class MissionInstaller : MonoInstaller
    {
        [SerializeField] private ResidentView _resident;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _entrance;
        [SerializeField] private MovementConfigSO _movement;

        public override void InstallBindings()
        {
            Container.Bind<ResidentView>().FromInstance(_resident);
            Container.Bind<MissionModel>().FromInstance(new MissionModel(_resident.ContentId));
            Container.BindInterfacesAndSelfTo<MissionController>().AsSingle();
            Container.Bind<MovementConfigSO>().FromInstance(_movement);
            Container.Bind<MissionInputModel>().AsSingle();
            Container.Bind<ToolSelectionModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<InteractionModel>().AsSingle();
            Container.Bind<QuestTargetModel>().FromInstance(new QuestTargetModel(_entrance.position));
            Container.Bind<Camera>().FromInstance(_camera);
        }
    }
}
