using TcpWtf.NumberSequence.Contracts.Ledger;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LedgerOperations
    {
        /// <summary>
        /// Create a new business.
        /// </summary>
        public async Task<Business> CreateBusinessAsync(Business business, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "ledger/businesses")
                {
                    Content = business.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Business>(cancellationToken);
        }

        /// <summary>
        /// Get an existing business.
        /// </summary>
        public async Task<Business> GetBusinessAsync(long id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"ledger/businesses/{id}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Business>(cancellationToken);
        }

        /// <summary>
        /// Get existing businesses.
        /// </summary>
        public async Task<List<Business>> GetBusinessesAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "ledger/businesses"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<Business>>(cancellationToken);
        }

        /// <summary>
        /// Update an existing business.
        /// </summary>
        public async Task<Business> UpdateBusinessAsync(Business business, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "ledger/businesses")
                {
                    Content = business.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Business>(cancellationToken);
        }
    }
}
