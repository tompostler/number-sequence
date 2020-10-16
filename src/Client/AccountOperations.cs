using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Account operations.
    /// Accounts are required to generate tokens which are required to interact with the remaining APIs.
    /// </summary>
    public sealed class AccountOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal AccountOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Create an account.
        /// </summary>
        public async Task<Account> CreateAsync(
            Account account,
            CancellationToken cancellationToken = default)
        {
            var response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "accounts")
                {
                    Content = account.ToJsonContent()
                },
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<Account>();
        }

        /// <summary>
        /// Get an account.
        /// </summary>
        public async Task<Account> GetAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            var response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"accounts/{name}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Account>();
        }
    }
}
