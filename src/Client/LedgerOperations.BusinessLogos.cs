using TcpWtf.NumberSequence.Contracts.Ledger;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LedgerOperations
    {
        /// <summary>
        /// Create a logo for an existing business.
        /// </summary>
        public async Task<BusinessLogo> CreateBusinessLogoAsync(long businessId, BusinessLogo logo, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"ledger/businesses/{businessId}/logo")
                {
                    Content = logo.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<BusinessLogo>(cancellationToken);
        }

        /// <summary>
        /// Update the logo for an existing business.
        /// </summary>
        public async Task<BusinessLogo> UpdateBusinessLogoAsync(long businessId, BusinessLogo logo, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"ledger/businesses/{businessId}/logo")
                {
                    Content = logo.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<BusinessLogo>(cancellationToken);
        }

        /// <summary>
        /// Get the logo for an existing business.
        /// </summary>
        public async Task<BusinessLogo> GetBusinessLogoAsync(long businessId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"ledger/businesses/{businessId}/logo"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<BusinessLogo>(cancellationToken);
        }

        /// <summary>
        /// Delete the logo for an existing business.
        /// </summary>
        public async Task DeleteBusinessLogoAsync(long businessId, CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"ledger/businesses/{businessId}/logo"),
                cancellationToken);
        }
    }
}
