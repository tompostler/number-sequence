using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Count operations.
    /// </summary>
    public sealed class CountOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal CountOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Create a new count.
        /// </summary>
        public async Task<Count> CreateAsync(Count count, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "counts")
                {
                    Content = count.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Count>(cancellationToken);
        }

        /// <summary>
        /// Get an existing count.
        /// </summary>
        public async Task<Count> GetAsync(string name, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"counts/{name}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Count>(cancellationToken);
        }

        /// <summary>
        /// Increment an existing count.
        /// </summary>
        public async Task<Count> IncrementAsync(string name, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"counts/{name}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Count>(cancellationToken);
        }

        /// <summary>
        /// Increment an existing count by a specific amount.
        /// </summary>
        public async Task<Count> IncrementByAmountAsync(string name, ulong amount, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"counts/{name}/{amount}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Count>(cancellationToken);
        }
    }
}
