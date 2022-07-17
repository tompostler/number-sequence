using System;
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
    }
}
