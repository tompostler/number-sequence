using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace number_sequence.Extensions
{
    public static class StreamExtensions
    {
        private static readonly JsonSerializerOptions options;
        static StreamExtensions()
        {
            options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
        }

        private class CosmosResponse<T>
        {
            public List<T> Documents { get; set; }
        }

        public static async Task<List<T>> ReadResultsAsync<T>(this Stream stream)
        {
            CosmosResponse<T> queryResponse = await JsonSerializer.DeserializeAsync<CosmosResponse<T>>(stream, options);
            return queryResponse.Documents;
        }
    }
}
