using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var cosmosClient = new CosmosClient("https://localhost:8081/", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
            var database = (await cosmosClient.CreateDatabaseIfNotExistsAsync("shared", 400)).Database;
            var container = (await database.CreateContainerIfNotExistsAsync("nstcpwtf", "/PK")).Container;
            await container.DeleteContainerAsync();
            await database.CreateContainerIfNotExistsAsync("nstcpwtf", "/PK");
            client = default;
            assemblyLogger.LogInformation(nameof(ResetCosmosEmulatorAsync));
        }

        [AssemblyInitialize]
        public static async Task AssemblyInitAsync(TestContext _)
        {
            UnauthedClient = new NsTcpWtfClient(LoggerFactory.CreateLogger<NsTcpWtfClient>(), default, Stamp.LocalDev);
            await ResetCosmosEmulatorAsync();
        }

        public static Account Account = new Account
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
