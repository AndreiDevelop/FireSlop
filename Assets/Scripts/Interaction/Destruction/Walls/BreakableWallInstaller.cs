using Zenject;

namespace FireSlop.Interaction
{
    public sealed class BreakableWallInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<BreakableWallModel>().AsSingle();
            Container.Bind<BreakableWallController>().AsSingle();
        }
    }
}
