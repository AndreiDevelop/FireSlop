using Zenject;

namespace FireSlop.Orientation
{
    public sealed class OrientationInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<OrientationModel>().AsSingle();
            Container.Bind<OrientationController>().AsSingle();
        }
    }
}
