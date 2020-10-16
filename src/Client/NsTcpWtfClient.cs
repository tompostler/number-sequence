using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Client for communciating with the REST APIs.
    /// </summary>
    public sealed class NsTcpWtfClient
    {
        private readonly ILogger<NsTcpWtfClient> logger;
        private readonly Task<string> tokenCallback;
        private readonly HttpClient client;

        private readonly string clientVersion;
        private readonly string clientName;

        /// <summary>
        /// Create a client.
        /// </summary>
        /// <param name="logger">Logger for any client logging. Logs requests and responses.</param>
        /// <param name="tokenCallback">A callback that will be invoked before any APIs requiring auth.</param>
        /// <param name="stamp">Override the base uri for unit testing or other hosting.</param>
        public NsTcpWtfClient(
            ILogger<NsTcpWtfClient> logger,
            Task<string> tokenCallback,
            Stamp stamp = Stamp.Public)
        {
            this.logger = logger;
            this.tokenCallback = tokenCallback;

            var baseUri = stamp switch
            {
                Stamp.LocalDev => new Uri("https://localhost:45678/"),
                Stamp.Public => new Uri("https://ns.tcp.wtf/"),
                _ => throw new ArgumentOutOfRangeException(nameof(stamp))
            };
            this.client = new HttpClient() { BaseAddress = baseUri };

            this.clientVersion = Assembly.GetAssembly(typeof(NsTcpWtfClient)).GetName().Version.ToString(3);
            this.clientName = Environment.MachineName;

            this.Account = new AccountOperations(this);
        }

        public AccountOperations Account { get; }
    }
}
