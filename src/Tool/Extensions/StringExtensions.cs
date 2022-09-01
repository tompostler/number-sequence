using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TcpWtf.NumberSequence.Tool.Extensions
{
    internal static class StringExtensions
    {
        private static readonly JsonSerializerOptions options;
        static StringExtensions()
        {
            options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
        }

        public static T FromBase64JsonString<T>(this string value) => JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(Convert.FromBase64String(value)), options);

        public static T FromJsonString<T>(this string value) => JsonSerializer.Deserialize<T>(value, options);
    }
}
