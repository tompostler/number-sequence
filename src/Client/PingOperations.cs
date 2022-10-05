using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Ping. Check if the service is online.
    /// </summary>
    public sealed class PingOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal PingOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Ping the service, and return the IP address as reported to the server.
        /// </summary>
        public async Task<string> GetPublicIpAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "ping/ip"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Ping the service.
        /// </summary>
        public async Task SendAsync(CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "ping"),
                cancellationToken,
                needsPreparation: false);
        }

        /// <summary>
        /// Ping the service, with auth.
        /// </summary>
        public async Task SendWithAuthAsync(CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "ping"),
                cancellationToken);
        }

        /// <summary>
        /// Ping the service, with auth, to the endpoint that requires a role.
        /// </summary>
        public async Task SendWithAuthToRoleAsync(CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "ping"),
                cancellationToken);
        }
    }
}
