using System;
using System.Globalization;

namespace FireSlop
{
    public static class CompactNumber
    {
        private static readonly string[] Suffixes = { "", "k", "M", "B", "T", "Qa", "Qi" };

        public static string Format(long value)
        {
            decimal scaled = value;
            int suffix = 0;
            while (Math.Abs(scaled) >= 1000m && suffix < Suffixes.Length - 1)
            {
                scaled /= 1000m;
                suffix++;
            }

            decimal rounded = decimal.Round(scaled, 1, MidpointRounding.AwayFromZero);
            if (Math.Abs(rounded) >= 1000m && suffix < Suffixes.Length - 1)
            {
                rounded /= 1000m;
                suffix++;
            }

            return rounded.ToString("0.#", CultureInfo.InvariantCulture) + Suffixes[suffix];
        }
    }
}
