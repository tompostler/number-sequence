using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Get status about the pdf background services.
    /// </summary>
    public sealed class PdfStatusOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal PdfStatusOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Get the current status; filtered server-side.
        /// </summary>
        public async Task<PdfStatus> GetAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "pdfstatus"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<PdfStatus>(cancellationToken: cancellationToken);
        }
    }
}
