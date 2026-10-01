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
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "accounts")
                {
                    Content = account.ToJsonContent()
                },
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadJsonAsAsync<Account>(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get an account.
        /// </summary>
        public async Task<Account> GetAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"accounts/{name}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Account>(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all accounts. Requires the <see cref="AccountRoles.Admin"/> role.
        /// </summary>
        public async Task<List<Account>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "accounts"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<Account>>(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Set an account's tier. Requires the <see cref="AccountRoles.Admin"/> role.
        /// </summary>
        public async Task<Account> UpdateTierAsync(
            string name,
            AccountTier tier,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"accounts/{Uri.EscapeDataString(name)}/tier/{tier}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Account>(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Grant an account a role from <see cref="AccountRoles"/>. Requires the <see cref="AccountRoles.Admin"/> role.
        /// </summary>
        public async Task<Account> AddRoleAsync(
            string name,
            string role,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"accounts/{Uri.EscapeDataString(name)}/roles/{Uri.EscapeDataString(role)}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Account>(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Remove a role from an account. Requires the <see cref="AccountRoles.Admin"/> role.
        /// </summary>
        public async Task<Account> RemoveRoleAsync(
            string name,
            string role,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"accounts/{Uri.EscapeDataString(name)}/roles/{Uri.EscapeDataString(role)}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Account>(cancellationToken: cancellationToken);
        }
    }
}
