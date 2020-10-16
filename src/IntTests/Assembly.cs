using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;

namespace number_sequence.IntTests
{
    [TestClass]
    public static class Assembly
    {
        public static NsTcpWtfClient Client { get; private set; }

        [AssemblyInitialize]
        public static async Task AssemblyInitAsync(TestContext context)
        {
            var cosmosClient = new CosmosClient("https://localhost:8081/", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
            var database = (await cosmosClient.CreateDatabaseIfNotExistsAsync("shared", 400)).Database;
            var container = (await database.CreateContainerIfNotExistsAsync("nstcpwtf", "/PK")).Container;
            await container.DeleteContainerAsync();
            await database.CreateContainerIfNotExistsAsync("nstcpwtf", "/PK");

            Client = new NsTcpWtfClient(new TypedNullLogger<NsTcpWtfClient>(), default, Stamp.LocalDev);
        }

        private class TypedNullLogger<T> : ILogger<T>
        {
            private static readonly ILogger NoOpLogger = NullLogger.Instance;

            public IDisposable BeginScope<TState>(TState state) => NoOpLogger.BeginScope(state);
            public bool IsEnabled(LogLevel logLevel) => NoOpLogger.IsEnabled(logLevel);

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                => NoOpLogger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
