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
    }
}
