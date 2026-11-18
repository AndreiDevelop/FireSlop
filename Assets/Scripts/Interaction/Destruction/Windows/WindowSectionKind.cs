using System;

namespace FireSlop.Interaction
{
    [Flags]
    internal enum WindowSectionKind
    {
        UpperGlass = 1 << 0,
        LowerGlass = 1 << 1,
        Wood = 1 << 2
    }
}
