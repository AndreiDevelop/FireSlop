namespace FireSlop.Interaction
{
    internal sealed class BreakableWindowModel
    {
        private WindowSectionKind _brokenSections;

        public WindowSectionKind Break(WindowSectionKind section)
        {
            var damaged = section == WindowSectionKind.Wood
                ? WindowSectionKind.Wood | WindowSectionKind.UpperGlass | WindowSectionKind.LowerGlass
                : section;
            var newlyBroken = damaged & ~_brokenSections;
            _brokenSections |= damaged;
            return newlyBroken;
        }
    }
}
