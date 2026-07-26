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
        /// Submit a canine record for pdf generation.
        /// </summary>
        public Task<ChiroInputCreated> SubmitCanineAsync(ChiroInput input, CancellationToken cancellationToken = default)
            => this.SubmitAsync("canine", input, cancellationToken);

        /// <summary>
        /// Submit an equine record for pdf generation.
        /// </summary>
        public Task<ChiroInputCreated> SubmitEquineAsync(ChiroInput input, CancellationToken cancellationToken = default)
            => this.SubmitAsync("equine", input, cancellationToken);

        private async Task<ChiroInputCreated> SubmitAsync(string species, ChiroInput input, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"chiro/{species}")
                {
                    Content = input.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<ChiroInputCreated>(cancellationToken);
        }
    }
}
