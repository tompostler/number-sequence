using Microsoft.AspNetCore.Mvc;
using number_sequence.Services.Libraries;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Controllers
{
    /// <summary>
    /// Lookups ask outside services and save nothing, so they aren't scoped to a library and need only the Library role.
    /// </summary>
    public sealed partial class LibraryController
    {
        /// <summary>
        /// A book by ISBN: one result, or none when neither Open Library nor Google Books has it.
        /// </summary>
        [HttpGet("lookup/isbn/{isbn}")]
        public async Task<IActionResult> LookupIsbnAsync(string isbn, CancellationToken cancellationToken)
        {
            if (!LibraryBarcode.TryParse(isbn, out LibraryBarcode barcode, out string error))
            {
                return this.BadRequest(error);
            }
            if (barcode.Kind != LibraryBarcodeKind.Isbn)
            {
                return this.BadRequest($"[{isbn}] isn't an ISBN. Movies and games are looked up by title.");
            }

            LibraryLookupResult result = await this.serviceProvider.GetRequiredService<LibraryLookup>().LookupIsbnAsync(barcode.Value, cancellationToken);
            this.logger.LogInformation($"[{this.AccountName}] looked up ISBN [{barcode.Value}]: {(result == null ? "not found" : $"{result.Source} [{result.SourceId}]")}.");
            return this.Ok(result == null ? new List<LibraryLookupResult>() : [result]);
        }

        /// <summary>
        /// Candidates for a title. Each may lack fields; fetch the chosen one with <see cref="GetLookupAsync"/>.
        /// </summary>
        [HttpGet("lookup/search")]
        public async Task<IActionResult> SearchLookupAsync([FromQuery] LibraryMediaType mediaType, [FromQuery] string q, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return this.BadRequest("Search for something.");
            }

            List<LibraryLookupResult> results = await this.serviceProvider.GetRequiredService<LibraryLookup>().SearchAsync(mediaType, q, cancellationToken);
            this.logger.LogInformation($"[{this.AccountName}] searched {mediaType} lookup for [{q}]: {results.Count} results.");
            return this.Ok(results);
        }

        [HttpGet("lookup/{source}/{sourceId}")]
        public async Task<IActionResult> GetLookupAsync(string source, string sourceId, CancellationToken cancellationToken)
        {
            LibraryLookupResult result = await this.serviceProvider.GetRequiredService<LibraryLookup>().GetAsync(source, sourceId, cancellationToken);
            this.logger.LogInformation($"[{this.AccountName}] looked up {source} [{sourceId}]: {(result == null ? "not found" : "found")}.");
            return result == null ? this.NotFound($"{source} has no record [{sourceId}].") : this.Ok(result);
        }

        /// <summary>
        /// A lookup result's cover as a thumbnail, so the add page can show it without linking to the provider. Re-encoded
        /// rather than passed through, so whatever a provider host returns is never served from this site as-is.
        /// </summary>
        [HttpGet("lookup/cover")]
        public async Task<IActionResult> GetLookupCoverAsync([FromQuery] string url, CancellationToken cancellationToken)
        {
            if (!LibraryCovers.IsAllowed(url))
            {
                return this.BadRequest("Covers are only fetched from lookup providers.");
            }

            LibraryCovers covers = this.serviceProvider.GetRequiredService<LibraryCovers>();
            byte[] data = await covers.FetchAsync(url, cancellationToken);
            LibraryCovers.Prepared prepared = data == null ? null : covers.Prepare(data);
            if (prepared == null)
            {
                return this.NotFound();
            }

            this.Response.Headers.CacheControl = "private, max-age=86400";
            this.Response.Headers.XContentTypeOptions = "nosniff";
            return this.File(prepared.Thumbnail, LibraryCovers.ContentType);
        }
    }
}
