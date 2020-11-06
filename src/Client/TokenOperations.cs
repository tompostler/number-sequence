using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Token operations.
    /// Tokens are required to interact with the remaining APIs.
    /// Because it's a personal project, tokens cannot be deleted. They only expire naturally.
    /// </summary>
    public sealed class TokenOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal TokenOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Create a token.
        /// </summary>
        public async Task<Token> CreateAsync(
            Token token,
            CancellationToken cancellationToken = default)
        {
            var response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "tokens")
                {
                    Content = token.ToJsonContent()
                },
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<Token>();
        }
    }
}
