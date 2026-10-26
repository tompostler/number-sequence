using TcpWtf.NumberSequence.Contracts.Libraries;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LibraryOperations
    {
        /// <summary>
        /// Look up a book by ISBN-10 or ISBN-13. Returns one result, or none when no provider has it. Nothing is saved.
        /// </summary>
        public async Task<List<LibraryLookupResult>> LookupIsbnAsync(string isbn, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/lookup/isbn/{Uri.EscapeDataString(isbn)}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<LibraryLookupResult>>(cancellationToken);
        }

        /// <summary>
        /// Search outside services by title. Candidates can lack fields; get the chosen one with <see cref="GetLookupAsync"/>.
        /// A media type with no lookup, or whose provider isn't configured on the server, is a 400 with the reason.
        /// </summary>
        public async Task<List<LibraryLookupResult>> SearchLookupAsync(LibraryMediaType mediaType, string q, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/lookup/search?mediaType={mediaType}&q={Uri.EscapeDataString(q)}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<LibraryLookupResult>>(cancellationToken);
        }

        /// <summary>
        /// Get the full record for a search candidate, by its <see cref="LibraryLookupResult.Source"/> and
        /// <see cref="LibraryLookupResult.SourceId"/>.
        /// </summary>
        public async Task<LibraryLookupResult> GetLookupAsync(string source, string sourceId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/lookup/{Uri.EscapeDataString(source)}/{Uri.EscapeDataString(sourceId)}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryLookupResult>(cancellationToken);
        }
    }
}
