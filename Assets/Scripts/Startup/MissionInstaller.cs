using System;
using FireSlop.Audio;
using FireSlop.Cats;
using FireSlop.Drone;
using FireSlop.Firefighter;
using FireSlop.Interaction;
using FireSlop.Mission;
using FireSlop.MissionHud;
using FireSlop.Tools;
using UnityEngine;
using Zenject;

namespace FireSlop.Startup
{
    public sealed class MissionInstaller : MonoInstaller
    {
        [SerializeField] private DroneController _drone;
        [SerializeField] private ResidentView _resident;
        [SerializeField] private RescueZoneView _rescueZone;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _entrance;
        [SerializeField] private MovementConfigSO _movement;
        [SerializeField] private ToolBeltSO _toolBelt;
        [SerializeField] private HudActionCatalogSO _hudActions;
        [SerializeField] private FireAudioConfigSO _fireAudio;

        public override void InstallBindings()
        {
            if (_hudActions == null)
            {
                throw new InvalidOperationException("Assign a HUD Action Catalog asset in Mission Installer.");
            }

            _hudActions.Validate();
            Container.Bind<HudActionCatalogSO>().FromInstance(_hudActions);

            // Tool-only prototype scenes do not contain a rescue mission.
            if (_resident != null)
            {
                Container.Bind<ResidentView>().FromInstance(_resident);
                Container.Bind<RescueZoneView>().FromInstance(_rescueZone);
                Container.Bind<MissionModel>().FromInstance(new MissionModel(_resident.ContentId));
                Container.BindInterfacesAndSelfTo<MissionController>().AsSingle();
            }

            if (TryGetComponent<RescueController>(out var carrier))
            {
                Container.Bind<RescueController>().FromInstance(carrier);
            }

            Container.Bind<DroneModel>().AsSingle();
            Container.Bind<CatFaceModel>().AsSingle();
            if (_drone != null)
            {
                Container.Bind<DroneController>().FromInstance(_drone);
            }

            Container.Bind<MovementConfigSO>().FromInstance(_movement);
            Container.Bind<HoseFlightModel>().AsSingle();
            Container.Bind<MissionInputModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<MissionPauseController>().AsSingle();
            Container.BindInterfacesAndSelfTo<MissionHudPlatform>().AsSingle();
            Container.Bind<SettingsModel>().AsSingle();
            if (_fireAudio == null || !_fireAudio.IsValid)
            {
                throw new InvalidOperationException("Assign valid Fire Audio settings in Mission Installer.");
            }

            Container.Bind<FireAudioConfigSO>().FromInstance(_fireAudio);
            Container.Bind<FireAudioModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<FireAudioController>().AsSingle();
            Container.Bind<SettingsController>().AsSingle().NonLazy();
            Container.Bind<ToolBeltSO>().FromInstance(_toolBelt);
            Container.Bind<ToolSelectionModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<InteractionModel>().AsSingle();
            Container.Bind<BrickDustEffect>().AsSingle();
            Container.Bind<DoorModel>().AsTransient();
            Container.Bind<OpenableModel>().AsTransient();
            Container.Bind<FireSpreadModel>().AsTransient();
            Container.Bind<WaterKnockdownModel>().AsTransient();
            Container.BindInterfacesAndSelfTo<QuestTargetModel>().AsSingle();
            Container.Bind<Transform>().WithId(QuestArrowView.EntranceId).FromInstance(_entrance);
            Container.Bind<Camera>().FromInstance(_camera);
        }
    }
}
