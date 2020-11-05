using System;
using System.Security.Cryptography;
using System.Text;

namespace number_sequence.Extensions
{
    public static class StringExtensions
    {
        public static string ComputeSHA256(this string value)
        {
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));

            // BitConverter averages 50% faster than using a StringBuilder with every byte.ToString("x2")
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
