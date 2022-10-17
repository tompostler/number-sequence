using System;

namespace TcpWtf.NumberSequence.Tool
{
    internal static class Input
    {
        public static string GetString(string prompt, string defaultVal = default)
        {
            Console.Write(prompt);
            if (defaultVal != default)
            {
                Console.Write($" (default {defaultVal})");
            }
            Console.Write(": ");

            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) && defaultVal != default)
            {
                return defaultVal;
            }
            else
            {
                return input;
            }
        }

        internal static DateTimeOffset GetDateTime(string prompt, DateTimeOffset defaultVal = default)
        {
            Console.Write(prompt);
            if (defaultVal != default)
            {
                Console.Write($" (default {defaultVal:u})");
            }
            Console.Write(": ");

            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) && defaultVal != default)
            {
                return defaultVal;
            }
            else
            {
                return DateTimeOffset.Parse(input);
            }
        }

        internal static decimal GetDecimal(string prompt, bool canDefault = true, decimal defaultVal = 0)
        {
            Console.Write(prompt);
            if (canDefault)
            {
                Console.Write($" (default {defaultVal})");
            }
            Console.Write(": ");

            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) && canDefault)
            {
                return defaultVal;
            }
            else
            {
                return decimal.Parse(input);
            }
        }

        internal static long GetLong(string prompt, bool canDefault = true, long defaultVal = 0)
        {
            Console.Write(prompt);
            if (canDefault)
            {
                Console.Write($" (default {defaultVal})");
            }
            Console.Write(": ");

            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) && canDefault)
            {
                return defaultVal;
            }
            else
            {
                return long.Parse(input);
            }
        }

        internal static ulong GetULong(string prompt, bool canDefault = true)
        {
            Console.Write(prompt);
            if (canDefault)
            {
                Console.Write(" (default 0)");
            }
            Console.Write(": ");

            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) && canDefault)
            {
                return 0;
            }
            else
            {
                return ulong.Parse(input);
            }
        }
    }
}
