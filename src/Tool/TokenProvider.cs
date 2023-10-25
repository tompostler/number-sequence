using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool
{
    internal static class TokenProvider
    {
        private static readonly FileInfo tokenFile = new(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "tcpwtf",
                "ns",
                "token.json"));

        public static void Upsert(string token, ILogger logger = default)
        {
            if (tokenFile.Exists)
            {
                logger?.LogInformation($"Token file {tokenFile.FullName} already exists with last modified {tokenFile.LastWriteTimeUtc:u}. Deleting it.");
                tokenFile.Delete();
            }
            else
            {
                tokenFile.Directory.Create();
            }

            using FileStream tokenFileStream = tokenFile.Create();
            using StreamWriter tokenFileWriter = new(tokenFileStream);
            tokenFileWriter.Write(new TokenFileModel { Token = token }.ToJsonString(indented: true));
            logger?.LogInformation($"Wrote token value to file {tokenFile.FullName}");
        }

        public static string Get(ILogger logger = default)
        {
            if (!tokenFile.Exists)
            {
                throw new FileNotFoundException("API requires auth and token file does not exist. Use 'ns token' to create/save one.", tokenFile.FullName);
            }

            logger?.LogInformation($"Reading token value from file {tokenFile.FullName} with last modified {tokenFile.LastWriteTimeUtc:u}.");
            using FileStream tokenFileStream = tokenFile.OpenRead();
            using StreamReader tokenFileReader = new(tokenFileStream);
            return tokenFileReader.ReadToEnd().FromJsonString<TokenFileModel>().Token;
        }

        public static TokenValue GetValue(ILogger logger = default) => Get(logger).FromBase64JsonString<TokenValue>();

        public static string GetAccount(ILogger logger = default) => GetValue(logger).Account;

        public static Task<string> GetAsync(CancellationToken _) => Task.FromResult(Get());

        internal sealed class TokenFileModel
        {
            public int Version { get; set; } = 1;
            public string Token { get; set; }
        }
        internal sealed class TokenValue
        {
            [JsonPropertyName("a")]
            public string Account { get; set; }
            [JsonPropertyName("n")]
            public string Name { get; set; }
            [JsonPropertyName("c")]
            public DateTimeOffset CreatedDate { get; set; }
            [JsonPropertyName("e")]
            public DateTimeOffset ExpirationDate { get; set; }
        }
    }
}
