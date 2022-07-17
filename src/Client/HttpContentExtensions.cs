using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace TcpWtf.NumberSequence.Client
{
    internal static class HttpContentExtensions
    {
        private static readonly JsonSerializerOptions options;

        static HttpContentExtensions()
        {
            options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
        }

        internal static async Task<T> ReadJsonAsAsync<T>(this HttpContent httpContent, CancellationToken cancellationToken = default)
        {
            try
            {
                using System.IO.Stream stream = await httpContent.ReadAsStreamAsync(cancellationToken);
                return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new NsTcpWtfClientException($"Failed to deserialize to {typeof(T).FullName}.", ex);
            }
        }

        internal static StringContent ToJsonContent(this object content)
        {
            // Serialize with the custom serializer
            // This is mainly to ensure enums are serialized as strings so we don't have to worry as much about adding new enum values always at the end of the enum
            return new StringContent(JsonSerializer.Serialize(content, options), Encoding.UTF8, "application/json");
        }
    }
}
