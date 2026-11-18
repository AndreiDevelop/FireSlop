namespace FireSlop.Interaction
{
    internal sealed class BreakableWindowModel
    {
        private const WindowSectionKind AllSections = WindowSectionKind.Wood |
            WindowSectionKind.UpperGlass | WindowSectionKind.LowerGlass;

        private WindowSectionKind _brokenSections;

        public bool IsOpen => _brokenSections == AllSections;

        public WindowSectionKind Break(WindowSectionKind section)
        {
            var damaged = section == WindowSectionKind.Wood
                ? AllSections
                : section;
            var newlyBroken = damaged & ~_brokenSections;
            _brokenSections |= damaged;
            return newlyBroken;
        }
    }
}
