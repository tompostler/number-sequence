using System;
using System.Security.Cryptography;
using System.Text;

namespace number_sequence.IntTests
{
    public static class Extensions
    {
        public static string ComputeMD5(this string value)
        {
            using var hashAlg = MD5.Create();
            byte[] hash = hashAlg.ComputeHash(Encoding.UTF8.GetBytes(value));

            // BitConverter averages 50% faster than using a StringBuilder with every byte.ToString("x2")
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
