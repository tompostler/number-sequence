using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Token operations.
    /// Tokens are required to interact with the remaining APIs.
    /// </summary>
    public sealed class TokenOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal TokenOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Create a token. If a request is submitted for an expired token, it will be overwritten with a new one.
        /// </summary>
        public async Task<Token> CreateAsync(
            Token token,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "tokens")
                {
                    Content = token.ToJsonContent()
                },
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<Token>(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a token.
        /// </summary>
        public async Task<Token> DeleteAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"tokens/{name}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Token>(cancellationToken: cancellationToken);
        }
    }
}
