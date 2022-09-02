using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Get status about the latex background services.
    /// </summary>
    public sealed class LatexStatusOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal LatexStatusOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Get the current status; filtered server-side.
        /// </summary>
        public async Task<LatexStatus> GetAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "latexstatus"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LatexStatus>(cancellationToken: cancellationToken);
        }
    }
}
