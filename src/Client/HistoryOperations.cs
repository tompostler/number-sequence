namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// History. Get the list of recent commits.
    /// </summary>
    public sealed class HistoryOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal HistoryOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Get the history from the service.
        /// </summary>
        public async Task<string> GetAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "history"),
                cancellationToken,
                needsPreparation: false);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}
