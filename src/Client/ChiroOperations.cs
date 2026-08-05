using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Chiro operations.
    /// </summary>
    public sealed class ChiroOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal ChiroOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Submit a record for pdf generation.
        /// </summary>
        public async Task<ChiroInputCreated> SubmitAsync(ChiroSpecies species, ChiroInput input, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"chiro/{species}".ToLowerInvariant())
                {
                    Content = input.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<ChiroInputCreated>(cancellationToken);
        }
    }
}
