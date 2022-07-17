using Microsoft.Azure.Cosmos;
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

        internal static async Task ResetCosmosEmulatorAsync()
        {
            // Get the connection information for the local collection in Azure
            using var sr = new StreamReader(typeof(Assembly).Assembly.GetManifestResourceStream(typeof(Assembly), "number_sequence.IntTests.appsettings.Development.json"));
            string appSettingsLocal = sr.ReadToEnd();
            Options.CosmosDB cosmosOptions = JToken.Parse(appSettingsLocal)[nameof(Options.CosmosDB)].ToObject<Options.CosmosDB>();

            var cosmosClient = new CosmosClient(cosmosOptions.Endpoint, cosmosOptions.Key);
            Database database = (await cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosOptions.DatabaseId, 400)).Database;
            Container container = (await database.CreateContainerIfNotExistsAsync(cosmosOptions.ContainerId, "/PK")).Container;
            _ = await container.DeleteContainerAsync();
            _ = await database.CreateContainerIfNotExistsAsync(cosmosOptions.ContainerId, "/PK");
            client = default;
            assemblyLogger.LogInformation(nameof(ResetCosmosEmulatorAsync));
        }

        [AssemblyInitialize]
        public static async Task AssemblyInitAsync(TestContext _)
        {
            UnauthedClient = new NsTcpWtfClient(LoggerFactory.CreateLogger<NsTcpWtfClient>(), default, Stamp.LocalDev);
            await ResetCosmosEmulatorAsync();
        }

        public static Account Account = new()
        {
            Name = "IntegrationTests",
            Key = string.Empty.ComputeMD5()
        };
        public static Token Token;
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
