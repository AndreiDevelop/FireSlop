using UnityEngine;

namespace FireSlop.Interaction
{
    internal sealed class BreakableWallController
    {
        private const int CrackedBrickCount = 2;
        private readonly BreakableWallModel _model;

        public BreakableWallController(BreakableWallModel model)
        {
            _model = model;
        }

        internal bool SelectCrackedBricks(int candidateCount)
        {
            if (candidateCount < CrackedBrickCount)
            {
                return false;
            }

            _model.SetCrackedBricks(Random.Range(0, candidateCount), Random.Range(0, candidateCount - 1));
            return true;
        }

        internal void ApplyHit(BreakableWallView wall, BreakableBrickView brick, int candidateIndex, Vector3 direction,
            InteractionConfigSO config)
        {
            if (_model.TryCollapse(candidateIndex))
            {
                wall.Collapse(direction, config);
            }
            else
            {
                brick.ReleaseDebris(direction, config);
            }

            wall.PlayBrickBreak();
        }
    }
}
