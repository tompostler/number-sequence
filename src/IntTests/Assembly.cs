using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.IntTests
{
    [TestClass]
    public static class Assembly
    {
        public static ILoggerFactory LoggerFactory { get; private set; }
        public static NsTcpWtfClient UnauthedClient { get; private set; }

        private static readonly ILogger assemblyLogger;

        static Assembly()
        {
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddDebug());
            assemblyLogger = LoggerFactory.CreateLogger(typeof(Assembly));
        }

        internal static async Task ResetDataStorageAsync()
        {
            // Get the connection information for the local collection in Azure
            using var sr = new StreamReader(typeof(Assembly).Assembly.GetManifestResourceStream("number_sequence.IntTests.appsettings.Development.json"));
            var appSettingsLocal = JToken.Parse(sr.ReadToEnd());

            Options.CosmosDB cosmosOptions = appSettingsLocal[nameof(Options.CosmosDB)].ToObject<Options.CosmosDB>();
            var cosmosClient = new CosmosClient(cosmosOptions.Endpoint, cosmosOptions.Key);
            Database database = (await cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosOptions.DatabaseId, 400)).Database;
            Container container = (await database.CreateContainerIfNotExistsAsync(cosmosOptions.ContainerId, "/PK")).Container;
            _ = await container.DeleteContainerAsync();
            _ = await database.CreateContainerIfNotExistsAsync(cosmosOptions.ContainerId, "/PK");

            Options.Sql sqlOptions = appSettingsLocal[nameof(Options.Sql)].ToObject<Options.Sql>();
            using SqlConnection sqlConnection = new(sqlOptions.ConnectionString);
            await sqlConnection.OpenAsync();
            foreach (string tableToDeleteFrom in new[] { "Accounts", "Tokens" })
            {
                assemblyLogger.LogInformation($"Deleting from {tableToDeleteFrom}");
                using (SqlCommand sqlCommand = new($"DELETE FROM {tableToDeleteFrom}", sqlConnection))
                {
                    assemblyLogger.LogInformation($"Deleted {await sqlCommand.ExecuteNonQueryAsync()} rows.");
                }
            }

            client = default;
            assemblyLogger.LogInformation(nameof(ResetDataStorageAsync));
        }

        [AssemblyInitialize]
        public static async Task AssemblyInitAsync(TestContext _)
        {
            UnauthedClient = new NsTcpWtfClient(LoggerFactory.CreateLogger<NsTcpWtfClient>(), default, Stamp.LocalDev);
            await ResetDataStorageAsync();
        }

        internal static Account Account = new()
        {
            Name = "IntegrationTests",
            Key = string.Empty.ComputeMD5()
        };
        internal static Token Token;
        private static NsTcpWtfClient client;
        public static NsTcpWtfClient Client
        {
            get
            {
                if (client == default)
                {
                    UnauthedClient.Account.CreateAsync(Account).Wait();
                    Token = UnauthedClient.Token.CreateAsync(new Token { Account = Account.Name, Key = Account.Key, Name = Account.Name }).Result;
                    client = new NsTcpWtfClient(LoggerFactory.CreateLogger<NsTcpWtfClient>(), (_) => Task.FromResult(Token.Value), Stamp.LocalDev);
                }
                return client;
            }
        }
    }
}
