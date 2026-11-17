using FireSlop.Drone;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.Mission;
using FireSlop.MissionHud;
using UnityEngine;
using Zenject;

namespace FireSlop.Startup
{
    public sealed class MissionInstaller : MonoInstaller
    {
        [SerializeField] private DroneController _drone;
        [SerializeField] private ResidentView _resident;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _entrance;
        [SerializeField] private MovementConfigSO _movement;

        public override void InstallBindings()
        {
            // Tool-only prototype scenes do not contain a rescue mission.
            if (_resident != null)
            {
                Container.Bind<ResidentView>().FromInstance(_resident);
                Container.Bind<MissionModel>().FromInstance(new MissionModel(_resident.ContentId));
                Container.BindInterfacesAndSelfTo<MissionController>().AsSingle();
            }

            Container.Bind<DroneModel>().AsSingle();
            if (_drone != null)
            {
                Container.Bind<DroneController>().FromInstance(_drone);
            }

            Container.Bind<MovementConfigSO>().FromInstance(_movement);
            Container.Bind<HoseFlightModel>().AsSingle();
            Container.Bind<MissionInputModel>().AsSingle();
            Container.Bind<ToolSelectionModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<InteractionModel>().AsSingle();
            Container.Bind<FireSpreadModel>().AsTransient();
            Container.Bind<WaterKnockdownModel>().AsTransient();
            Container.Bind<QuestTargetModel>().FromInstance(new QuestTargetModel(_entrance.position));
            Container.Bind<Camera>().FromInstance(_camera);
        }
    }
}
