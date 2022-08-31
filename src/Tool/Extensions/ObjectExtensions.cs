using System.Text.Json;
using System.Text.Json.Serialization;

namespace TcpWtf.NumberSequence.Tool.Extensions
{
    internal static class ObjectExtensions
    {
        private static readonly JsonSerializerOptions options;
        static ObjectExtensions()
        {
            options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            options.WriteIndented = true;
        }

        public static string ToJsonString(this object value) => JsonSerializer.Serialize(value, options);
    }
}
