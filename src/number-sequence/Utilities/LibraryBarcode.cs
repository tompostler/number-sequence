using System.Linq.Expressions;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Utilities
{
    /// <summary>
    /// What kind of code a <see cref="LibraryBarcode"/> is.
    /// </summary>
    public enum LibraryBarcodeKind
    {
        /// <summary>
        /// A book, as ISBN-13 (EAN-13 in the 978/979 "Bookland" prefixes). ISBN-10 input is converted to this.
        /// </summary>
        Isbn,

        /// <summary>
        /// Any other retail product code, as GTIN-13. UPC-A input gets a leading zero, which is the same code a scanner
        /// reports when it reads a UPC-A as EAN-13.
        /// </summary>
        Gtin,

        /// <summary>
        /// Anything else: an 8-digit code, or a label of your own such as a Code 128 or QR sticker. Stored uppercased, with
        /// spaces and dashes removed, and otherwise as read.
        /// </summary>
        Other,
    }

    /// <summary>
    /// The one place barcodes are normalized. Every barcode written to <c>LibraryCopy.Barcode</c> or
    /// <c>LibraryScanEntry.Barcode</c>, and every barcode searched for, goes through <see cref="TryParse"/>. A path that
    /// skips it stores a form the others don't match, and lookups quietly miss.
    /// <para>
    /// Product codes are all brought to 13 digits because the same physical barcode reaches the server in several forms. A
    /// book scanned from the cover is an ISBN-13, but typed in from the copyright page it is often an ISBN-10. A movie's
    /// UPC-A comes back as 12 digits from one scanner and 13 (leading zero) from another.
    /// </para>
    /// <para>
    /// 8-digit codes are left alone. UPC-E and EAN-8 are both 8 digits, and the digits alone don't say which one was
    /// read, so expanding UPC-E to UPC-A would mangle every EAN-8. Neither is common on books, movies or games.
    /// </para>
    /// </summary>
    public readonly record struct LibraryBarcode(string Value, LibraryBarcodeKind Kind)
    {
        /// <summary>
        /// Matches the column length of <c>LibraryCopy.Barcode</c>.
        /// </summary>
        public const int MaxLength = 32;

        /// <summary>
        /// Normalizes <paramref name="input"/>. Fails, with a reason meant for the user, when it is empty, too long, or
        /// shaped like an ISBN or product code but with a wrong check digit. A wrong check digit is nearly always a typo
        /// or a misread, and storing it would make a copy that its real barcode never finds.
        /// </summary>
        public static bool TryParse(string input, out LibraryBarcode barcode, out string error)
        {
            barcode = default;
            error = null;

            string compact = new string((input ?? string.Empty).Where(x => !char.IsWhiteSpace(x) && x != '-').ToArray()).ToUpperInvariant();
            if (compact.Length == 0)
            {
                error = "Barcode is empty.";
                return false;
            }
            if (compact.Length > MaxLength)
            {
                error = $"Barcode is longer than {MaxLength} characters.";
                return false;
            }

            bool allDigits = compact.All(char.IsAsciiDigit);

            // ISBN-10: nine digits and a check character that may be X.
            if (compact.Length == 10 && compact.Take(9).All(char.IsAsciiDigit) && (char.IsAsciiDigit(compact[9]) || compact[9] == 'X'))
            {
                if (!IsValidIsbn10(compact))
                {
                    error = $"[{input}] looks like an ISBN-10 but its check digit is wrong.";
                    return false;
                }
                string isbn13 = "978" + compact[..9];
                barcode = new(isbn13 + GtinCheckDigit(isbn13), LibraryBarcodeKind.Isbn);
                return true;
            }

            if (allDigits && (compact.Length == 12 || compact.Length == 13))
            {
                if (!HasValidGtinCheckDigit(compact))
                {
                    error = $"[{input}] looks like a {(compact.Length == 12 ? "UPC" : "EAN/ISBN-13")} but its check digit is wrong.";
                    return false;
                }
                string gtin13 = compact.PadLeft(13, '0');
                LibraryBarcodeKind kind = gtin13.StartsWith("978") || gtin13.StartsWith("979") ? LibraryBarcodeKind.Isbn : LibraryBarcodeKind.Gtin;
                barcode = new(gtin13, kind);
                return true;
            }

            barcode = new(compact, LibraryBarcodeKind.Other);
            return true;
        }

        /// <summary>
        /// <see cref="TryParse"/> for a field or prompt that only takes an ISBN, which also accepts an ISBN-10 without its
        /// check character: those places use the phone's numeric keypad, which has no X, so "ends in X? leave it off". It is
        /// safe because converting to ISBN-13 discards the ISBN-10 check and computes a new one. Only here: anywhere a
        /// code could be anything, nine digits stay an <see cref="LibraryBarcodeKind.Other"/> label.
        /// </summary>
        public static bool TryParseIsbn(string input, out LibraryBarcode isbn, out string error)
        {
            string compact = new((input ?? string.Empty).Where(x => !char.IsWhiteSpace(x) && x != '-').ToArray());
            if (compact.Length == 9 && compact.All(char.IsAsciiDigit))
            {
                string isbn13 = "978" + compact;
                isbn = new(isbn13 + GtinCheckDigit(isbn13), LibraryBarcodeKind.Isbn);
                error = null;
                return true;
            }
            if (!TryParse(input, out isbn, out error))
            {
                return false;
            }
            if (isbn.Kind != LibraryBarcodeKind.Isbn)
            {
                error = $"[{input}] isn't an ISBN.";
                isbn = default;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Copies a normalized code identifies: by <see cref="LibraryCopy.Barcode"/>, or by <see cref="LibraryCopy.Isbn"/>
        /// for a book whose printed barcode is a UPC. Every query that finds copies by a code uses this, so a book found by
        /// its UPC in one place is also found by its ISBN in every other.
        /// </summary>
        public static Expression<Func<LibraryCopy, bool>> MatchesCopy(string normalized)
            => x => x.Barcode == normalized || x.Isbn == normalized;

        /// <summary>
        /// <see cref="MatchesCopy"/> for several codes at once, such as every code in a scan session.
        /// </summary>
        public static Expression<Func<LibraryCopy, bool>> MatchesAnyCopy(IReadOnlyCollection<string> normalized)
            => x => normalized.Contains(x.Barcode) || normalized.Contains(x.Isbn);

        private static bool IsValidIsbn10(string isbn10)
        {
            int sum = 0;
            for (int i = 0; i < 10; i++)
            {
                int digit = isbn10[i] == 'X' ? 10 : isbn10[i] - '0';
                sum += (10 - i) * digit;
            }
            return sum % 11 == 0;
        }

        private static bool HasValidGtinCheckDigit(string gtin)
            => GtinCheckDigit(gtin[..^1]) == gtin[^1];

        /// <summary>
        /// The GS1 check digit for the given digits, which works for any GTIN length: weights alternate 3 and 1 starting from
        /// the rightmost digit.
        /// </summary>
        private static char GtinCheckDigit(string digitsWithoutCheck)
        {
            int sum = 0;
            for (int i = 0; i < digitsWithoutCheck.Length; i++)
            {
                int digit = digitsWithoutCheck[digitsWithoutCheck.Length - 1 - i] - '0';
                sum += digit * (i % 2 == 0 ? 3 : 1);
            }
            return (char)('0' + (10 - sum % 10) % 10);
        }
    }
}
