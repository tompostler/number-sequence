using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Daily Sequence Value (DSV) operations.
    /// </summary>
    public sealed class DailySequenceValueOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal DailySequenceValueOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Create a new DSV.
        /// </summary>
        public async Task<DailySequenceValue> CreateAsync(DailySequenceValue dsv, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "dailysequencevalues")
                {
                    Content = dsv.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<DailySequenceValue>(cancellationToken);
        }

        /// <summary>
        /// Delete an existing DSV.
        /// </summary>
        public async Task<DailySequenceValue> DeleteAsync(string category, DateOnly date, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"dailysequencevalues/{category}/{date:yyyy-MM-dd}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<DailySequenceValue>(cancellationToken);
        }

        /// <summary>
        /// Get an existing DSV.
        /// </summary>
        public async Task<DailySequenceValue> GetAsync(string category, DateOnly date, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"dailysequencevalues/{category}/{date:yyyy-MM-dd}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<DailySequenceValue>(cancellationToken);
        }

        /// <summary>
        /// List all existing DSVs.
        /// </summary>
        public async Task<List<DailySequenceValue>> ListAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "dailysequencevalues"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<DailySequenceValue>>(cancellationToken);
        }

        /// <summary>
        /// Get DSVs for a category.
        /// </summary>
        public async Task<List<DailySequenceValue>> ListAsync(string category, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"dailysequencevalues/{category}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<DailySequenceValue>>(cancellationToken);
        }

        /// <summary>
        /// Update an existing DSV.
        /// Ignores the config validation and lets you update to anything.
        /// </summary>
        public async Task<DailySequenceValue> UpdateAsync(DailySequenceValue dsv, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "dailysequencevalues")
                {
                    Content = dsv.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<DailySequenceValue>(cancellationToken);
        }
    }
}
