using TcpWtf.NumberSequence.Contracts.Ledger;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LedgerOperations
    {
        /// <summary>
        /// Create a new statement.
        /// </summary>
        public async Task<Statement> CreateStatementAsync(Statement statement, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "ledger/statements")
                {
                    Content = statement.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Statement>(cancellationToken);
        }

        /// <summary>
        /// Get an existing statement.
        /// </summary>
        public async Task<Statement> GetStatementAsync(long id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"ledger/statements/{id}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Statement>(cancellationToken);
        }

        /// <summary>
        /// Get existing statements.
        /// </summary>
        public async Task<List<Statement>> GetStatementsAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "ledger/statements"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<Statement>>(cancellationToken);
        }

        /// <summary>
        /// Update an existing statement for processing.
        /// </summary>
        public async Task<Statement> UpdateStatementForProcessAsync(long id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"ledger/statements/{id}/process"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Statement>(cancellationToken);
        }
    }
}
