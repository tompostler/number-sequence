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

        public static string GetSHA256(this string @this)
        {
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(@this));

            // BitConverter averages 50% faster than using a StringBuilder with every byte.ToString("x2")
            string hashHex = BitConverter.ToString(hash).Replace("-", "").ToLower();

            // A SHA256 hash is 64 characters long
            return hashHex;
        }

        /// <summary>
        /// Converts a long input string (such as one from <see cref="GetSHA256(string)"/>) to 4 groups of 4.
        /// If the string is null or empty, it is returned instead.
        /// If it is shorter than 16 chars, dashes are added where possible.
        /// Replaces spaces with empty string.
        /// <br/>
        /// E.g. 921f32914faedaea00b1c4b6577ffff7a1885c3b3e20ea35359339a741c0ede6 becomes 921f-3291-4fae-daea.
        /// </summary>
        public static string MakeHumanFriendly(this string @this)
        {
            if (string.IsNullOrEmpty(@this))
            {
                return @this;
            }
            @this = @this.Replace(" ", string.Empty).ToLower();
            if (@this.Length > 16)
            {
                return $"{@this.Substring(0, 4)}-{@this.Substring(4, 4)}-{@this.Substring(8, 4)}-{@this.Substring(12, 4)}";
            }
            else if (@this.Length > 12)
            {
                return $"{@this.Substring(0, 4)}-{@this.Substring(4, 4)}-{@this.Substring(8, 4)}-{@this.Substring(12)}";
            }
            else if (@this.Length > 8)
            {
                return $"{@this.Substring(0, 4)}-{@this.Substring(4, 4)}-{@this.Substring(8)}";
            }
            else if (@this.Length > 4)
            {
                return $"{@this.Substring(0, 4)}-{@this.Substring(4)}";
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
