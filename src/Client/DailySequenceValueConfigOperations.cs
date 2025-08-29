using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Daily Sequence Value Config (DSVC) operations.
    /// </summary>
    public sealed class DailySequenceValueConfigOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal DailySequenceValueConfigOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Create a new DSVC.
        /// </summary>
        public async Task<DailySequenceValueConfig> CreateAsync(DailySequenceValueConfig dsvc, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "dailysequencevalueconfigs")
                {
                    Content = dsvc.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<DailySequenceValueConfig>(cancellationToken);
        }

        /// <summary>
        /// Delete an existing DSVC.
        /// </summary>
        public async Task<DailySequenceValueConfig> DeleteAsync(string category, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"dailysequencevalueconfigs/{category}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<DailySequenceValueConfig>(cancellationToken);
        }

        /// <summary>
        /// Get an existing DSVC.
        /// </summary>
        public async Task<DailySequenceValueConfig> GetAsync(string category, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"dailysequencevalueconfigs/{category}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<DailySequenceValueConfig>(cancellationToken);
        }

        /// <summary>
        /// List all existing DSVCs.
        /// </summary>
        public async Task<List<DailySequenceValueConfig>> ListAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "dailysequencevalueconfigs"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<DailySequenceValueConfig>>(cancellationToken);
        }

        /// <summary>
        /// Update an existing DSVC.
        /// </summary>
        public async Task<DailySequenceValueConfig> UpdateAsync(DailySequenceValueConfig dsvc, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "dailysequencevalueconfigs")
                {
                    Content = dsvc.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<DailySequenceValueConfig>(cancellationToken);
        }
    }
}
