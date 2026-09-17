using System.Globalization;

namespace number_sequence.Extensions
{
    public static class DecimalExtensions
    {
        /// <summary>
        /// Ledger amounts are always US dollars, so the symbol is part of the value and not a formatting choice.
        /// The "C" format takes its symbol from the ambient culture, which renders a placeholder on a host with no locale set (any linux box without LANG) and the wrong currency entirely on one with a different locale.
        /// </summary>
        public static string ToCurrencyDisplay(this decimal @this) => $"${@this.ToString("F2", CultureInfo.InvariantCulture)}";
    }
}
