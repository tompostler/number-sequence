using Microsoft.Azure.Cosmos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace number_sequence.IntTests
{
    [TestClass]
    public sealed class Assembly
    {
        [AssemblyInitialize]
        public static async Task AssemblyInitAsync()
        {
            var cosmosClient = new CosmosClient("https://localhost:8081/", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
            var database = (await cosmosClient.CreateDatabaseIfNotExistsAsync("shared", 400)).Database;
            var container = (await database.CreateContainerIfNotExistsAsync("nstcpwtf", "/PK")).Container;
            await container.DeleteContainerAsync();
            await database.CreateContainerIfNotExistsAsync("nstcpwtf", "/PK");
        }
    }
}
