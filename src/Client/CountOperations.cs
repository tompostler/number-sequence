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

        /// <summary>
        /// List all existing counts.
        /// </summary>
        public async Task<List<Count>> ListAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "counts"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<Count>>(cancellationToken);
        }

        /// <summary>
        /// Get events for an existing count, optionally filtered by date range.
        /// </summary>
        public async Task<List<CountEvent>> GetEventsAsync(string name, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken cancellationToken = default)
        {
            List<string> queryParams = new();
            if (from.HasValue)
            {
                queryParams.Add($"from={Uri.EscapeDataString(from.Value.ToString("o"))}");
            }
            if (to.HasValue)
            {
                queryParams.Add($"to={Uri.EscapeDataString(to.Value.ToString("o"))}");
            }
            string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"counts/{name}/events{queryString}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<CountEvent>>(cancellationToken);
        }

        /// <summary>
        /// Update the overflow drops oldest events setting for an existing count.
        /// </summary>
        public async Task<Count> UpdateOverflowDropsOldestEventsAsync(string name, bool overflowDropsOldestEvents, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"counts/{name}/OverflowDropsOldestEvents")
                {
                    Content = overflowDropsOldestEvents.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Count>(cancellationToken);
        }
    }
}
