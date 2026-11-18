namespace FireSlop.Interaction
{
    internal sealed class BreakableWallModel
    {
        private int _firstCracked = -1;
        private int _secondCracked = -1;
        private bool _hasCollapsed;

        internal void SetCrackedBricks(int first, int secondFromRemaining)
        {
            _firstCracked = first;
            _secondCracked = secondFromRemaining >= first ? secondFromRemaining + 1 : secondFromRemaining;
        }

        internal bool IsCracked(int index)
        {
            return index >= 0 && (index == _firstCracked || index == _secondCracked);
        }

        internal bool TryCollapse(int struckIndex)
        {
            if (_hasCollapsed || !IsCracked(struckIndex))
            {
                return false;
            }

            _hasCollapsed = true;
            return true;
        }
    }
}
