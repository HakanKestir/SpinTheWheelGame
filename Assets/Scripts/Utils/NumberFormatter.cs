using System.Globalization;

namespace WheelOfFortune.Utils
{
    /// <summary>
    /// Formats large gameplay amounts into compact labels so they always fit inside
    /// fixed-size UI areas (for example 2500 -> "2.5K", 1500000 -> "1.5M").
    /// </summary>
    public static class NumberFormatter
    {
        private static readonly string[] SUFFIXES = { string.Empty, "K", "M", "B", "T" };

        public static string ToShortString(long value)
        {
            if (value < 0)
            {
                return "-" + ToShortString(-value);
            }

            if (value < 1000)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            double scaledValue = value;
            int suffixIndex = 0;

            while (scaledValue >= 1000d && suffixIndex < SUFFIXES.Length - 1)
            {
                scaledValue /= 1000d;
                suffixIndex++;
            }

            string number = scaledValue.ToString("0.#", CultureInfo.InvariantCulture);
            return number + SUFFIXES[suffixIndex];
        }
    }
}
