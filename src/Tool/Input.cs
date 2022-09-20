using System;

namespace TcpWtf.NumberSequence.Tool
{
    internal static class Input
    {
        public static string GetString(string prompt)
        {
            Console.Write(prompt);
            Console.Write(": ");
            return Console.ReadLine();
        }

        internal static DateTimeOffset GetDateTime(string prompt)
        {
            Console.Write(prompt);
            Console.Write(": ");
            return DateTimeOffset.Parse(Console.ReadLine());
        }

        internal static decimal GetDecimal(string prompt, bool canDefault = true, decimal defaultVal = 0)
        {
            Console.Write(prompt);
            Console.Write($" (default {defaultVal}): ");

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

        internal static long GetLong(string prompt, bool canDefault = true)
        {
            Console.Write(prompt);
            Console.Write(" (default 0): ");

            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) && canDefault)
            {
                return 0;
            }
            else
            {
                return long.Parse(input);
            }
        }

        internal static ulong GetULong(string prompt, bool canDefault = true)
        {
            Console.Write(prompt);
            Console.Write(" (default 0): ");

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
