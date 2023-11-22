using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Redirect operations.
    /// </summary>
    public sealed class RedirectOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal RedirectOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Create a new redirect.
        /// </summary>
        public async Task<Redirect> CreateAsync(Redirect redirect, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "redirects")
                {
                    Content = redirect.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Redirect>(cancellationToken);
        }

        /// <summary>
        /// List all existing redirects.
        /// </summary>
        public async Task<List<Redirect>> ListAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "redirects"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<Redirect>>(cancellationToken);
        }
    }
}
