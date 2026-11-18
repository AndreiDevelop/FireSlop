using FireSlop.Mission;
using Zenject;

namespace FireSlop.Orientation
{
    public sealed class OrientationController
    {
        private readonly OrientationModel _model;
        private readonly MissionPauseController _pause;

        public OrientationController(OrientationModel model, [InjectOptional] MissionPauseController pause)
        {
            _model = model;
            _pause = pause;
        }

        public void Refresh(bool isMobile, bool isPortrait)
        {
            var shouldBlock = isMobile && isPortrait;
            _pause?.SetOrientationBlocked(shouldBlock);
            _model.SetBlocking(shouldBlock);
        }
    }
}
