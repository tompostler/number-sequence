using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;

namespace number_sequence.IntTests
{
    [TestClass]
    public static class Assembly
    {
        public static NsTcpWtfClient Client { get; private set; }

        private static readonly ILoggerFactory loggerFactory;

        static Assembly()
        {
            loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        }

        [AssemblyInitialize]
        public static async Task AssemblyInitAsync(TestContext _)
        {
            var cosmosClient = new CosmosClient("https://localhost:8081/", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
            var database = (await cosmosClient.CreateDatabaseIfNotExistsAsync("shared", 400)).Database;
            var container = (await database.CreateContainerIfNotExistsAsync("nstcpwtf", "/PK")).Container;
            await container.DeleteContainerAsync();
            await database.CreateContainerIfNotExistsAsync("nstcpwtf", "/PK");

            Client = new NsTcpWtfClient(loggerFactory.CreateLogger<NsTcpWtfClient>(), default, Stamp.LocalDev);
        }
    }
}
