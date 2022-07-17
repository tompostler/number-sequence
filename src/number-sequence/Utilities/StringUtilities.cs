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
    }
}
