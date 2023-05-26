using System.Security.Cryptography;
using System.Text;

namespace number_sequence.Utilities
{
    public static class StringUtilities
    {
        public static string GetRandomAlphanumericString(byte length = 128)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                const string validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);

                var stringBuilder = new StringBuilder();
                foreach (byte randomByte in randomBytes)
                {
                    _ = stringBuilder.Append(validCharacters[randomByte % validCharacters.Length]);
                }

                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// Converts a long input string (such as one from <see cref="Unlimitedinf.Utilities.Extensions.StringExtensions.ComputeSHA256(string)"/>) to 3 groups of 3.
        /// If the string is null or empty, it is returned instead.
        /// If it is shorter than 9 chars, dashes are added where possible.
        /// Replaces spaces with empty string.
        /// <br/>
        /// E.g. 921f32914faedaea00b1c4b6577ffff7a1885c3b3e20ea35359339a741c0ede6 becomes 921-f32-914.
        /// </summary>
        public static string MakeHumanFriendly(this string @this)
        {
            if (string.IsNullOrEmpty(@this))
            {
                return @this;
            }
            @this = @this.Replace(" ", string.Empty).ToLower();
            if (@this.Length > 9)
            {
                return $"{@this.Substring(0, 3)}-{@this.Substring(3, 3)}-{@this.Substring(6, 3)}";
            }
            else if (@this.Length > 6)
            {
                return $"{@this.Substring(0, 3)}-{@this.Substring(3, 3)}-{@this.Substring(6)}";
            }
            else if (@this.Length > 3)
            {
                return $"{@this.Substring(0, 3)}-{@this.Substring(3)}";
            }
            else // if (@this.Length > 0)
            {
                return @this;
            }
        }

        public static string EscapeForLatex(this string @this)
        {
            if (string.IsNullOrEmpty(@this))
            {
                return @this;
            }
            else
            {
                StringBuilder sb = new(@this.Length);
                foreach (char letter in @this)
                {
                    _ = letter switch
                    {
                        '&' or '%' or '$' or '#' or '_' or '{' or '}' => sb.Append(@"\" + letter),
                        '~' => sb.Append(@"{\textasciitilde}"),
                        '^' => sb.Append(@"{\textasciicircum}"),
                        '\\' => sb.Append(@"{\textbackslash}"),
                        _ => sb.Append(letter),
                    };
                }
                return sb.ToString();
            }
        }
    }
}
