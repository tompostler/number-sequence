using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
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

            Options.Sql sqlOptions = appSettingsLocal[nameof(Options.Sql)].ToObject<Options.Sql>();
            using SqlConnection sqlConnection = new(sqlOptions.ConnectionString);
            await sqlConnection.OpenAsync();
            foreach (string tableToDeleteFrom in new[] { "Accounts", "Counts", "Tokens" })
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
