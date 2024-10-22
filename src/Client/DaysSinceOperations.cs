using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Days since operations.
    /// </summary>
    public sealed class DaysSinceOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal DaysSinceOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Create a new days since.
        /// </summary>
        public async Task<DaysSince> CreateAsync(DaysSince daysSince, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "dayssinces")
                {
                    Content = daysSince.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<DaysSince>(cancellationToken);
        }

        /// <summary>
        /// Get an existing days since.
        /// </summary>
        public async Task<DaysSince> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"dayssinces/{id}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<DaysSince>(cancellationToken);
        }

        /// <summary>
        /// List all existing days since.
        /// </summary>
        public async Task<List<DaysSince>> ListAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "dayssinces"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<DaysSince>>(cancellationToken);
        }

        /// <summary>
        /// Update a days since.
        /// </summary>
        public async Task<DaysSince> UpdateAsync(DaysSince daysSince, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "dayssinces")
                {
                    Content = daysSince.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<DaysSince>(cancellationToken);
        }
    }
}
