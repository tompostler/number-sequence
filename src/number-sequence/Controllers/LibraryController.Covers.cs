using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Exceptions;
using number_sequence.Services.Libraries;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Controllers
{
    /// <summary>
    /// Covers live in blob storage, not sql: a few thousand at up to 100 KB each don't belong in the database. Each item
    /// has a cover and a thumbnail, both WebP images drawn by <see cref="LibraryCovers"/>.
    /// </summary>
    public sealed partial class LibraryController
    {
        /// <summary>
        /// The stored cover, or its thumbnail, streamed from blob storage rather than read into memory first, since a page of
        /// cards asks for dozens at once. Cached for a month: the pages link it with the item's modified date in the query,
        /// and setting or removing a cover changes that date.
        /// </summary>
        [HttpGet("libraries/{libraryId}/items/{itemId}/cover")]
        public async Task<IActionResult> GetItemCoverAsync(long libraryId, long itemId, CancellationToken cancellationToken, [FromQuery] bool thumbnail = false)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Viewer, cancellationToken);
            if (!await nsContext.LibraryItems.AnyAsync(x => x.LibraryId == libraryId && x.Id == itemId && x.HasCover, cancellationToken))
            {
                return this.NotFound($"Item [{itemId}] in library [{libraryId}] has no cover.");
            }

            BlobClient blob = await this.serviceProvider.GetRequiredService<NsStorage>().GetLibraryCoverBlobClientAsync(itemId, thumbnail, cancellationToken);
            BlobDownloadStreamingResult download;
            try
            {
                download = (await blob.DownloadStreamingAsync(cancellationToken: cancellationToken)).Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                this.logger.LogWarning($"Item [{itemId}] has a cover but no [{blob.Name}] blob.");
                return this.NotFound($"Item [{itemId}] in library [{libraryId}] has no cover.");
            }

            // The blob's stream can't seek, so the length isn't known from it.
            this.Response.ContentLength = download.Details.ContentLength;
            this.Response.Headers.CacheControl = "private, max-age=2592000, immutable";
            this.Response.Headers.XContentTypeOptions = "nosniff";
            // Copied to the response as it arrives, then disposed by the result, which closes the download.
            return this.File(download.Content, LibraryCovers.ContentType);
        }

        /// <summary>
        /// Sets an item's cover from an uploaded image or from a lookup result's cover urls. Returns the item, without its
        /// copies.
        /// </summary>
        [HttpPut("libraries/{libraryId}/items/{itemId}/cover")]
        public async Task<IActionResult> SetItemCoverAsync(long libraryId, long itemId, [FromBody] LibraryCover cover, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryItem itemRecord = await nsContext.LibraryItems.SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.Id == itemId, cancellationToken);
            if (itemRecord == default)
            {
                return this.NotFound($"Item [{itemId}] not found in library [{libraryId}].");
            }

            LibraryCovers covers = this.serviceProvider.GetRequiredService<LibraryCovers>();
            LibraryCovers.Prepared prepared;
            string from;
            if (cover?.Data?.Length > 0)
            {
                if (cover.Data.Length > LibraryCovers.MaxInputBytes)
                {
                    return this.BadRequest($"The image is {cover.Data.Length:N0} bytes; the most accepted is {LibraryCovers.MaxInputBytes:N0}.");
                }
                prepared = covers.Prepare(cover.Data) ?? throw new BadRequestException("That isn't an image that can be read. Use a JPEG, PNG, GIF or WebP.");
                from = $"an upload of {cover.Data.Length:N0} bytes";
            }
            else if (cover?.SourceUrls?.Count > 0)
            {
                if (!cover.SourceUrls.All(LibraryCovers.IsAllowed))
                {
                    return this.BadRequest("Covers are only fetched from lookup providers.");
                }
                prepared = await covers.FetchAndPrepareAsync(cover.SourceUrls, cancellationToken)
                    ?? throw new FailedDependencyException("None of the cover images could be fetched.");
                from = string.Join(", ", cover.SourceUrls);
            }
            else
            {
                return this.BadRequest("Send either the image or the urls to fetch it from.");
            }

            await this.StoreCoverAsync(itemRecord, prepared, cancellationToken);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] set the cover of item [{itemId}] in library [{libraryId}] from {from}.");
            return this.Ok(itemRecord);
        }

        /// <summary>
        /// Uploads both blobs and marks the tracked <paramref name="itemRecord"/> as having a cover; the caller saves. Shared
        /// by setting a cover and adding an item from a scan.
        /// </summary>
        private async Task StoreCoverAsync(LibraryItem itemRecord, LibraryCovers.Prepared prepared, CancellationToken cancellationToken)
        {
            NsStorage storage = this.serviceProvider.GetRequiredService<NsStorage>();
            BlobHttpHeaders headers = new() { ContentType = LibraryCovers.ContentType };
            BlobClient coverBlob = await storage.GetLibraryCoverBlobClientAsync(itemRecord.Id, thumbnail: false, cancellationToken);
            BlobClient thumbnailBlob = await storage.GetLibraryCoverBlobClientAsync(itemRecord.Id, thumbnail: true, cancellationToken);
            _ = await coverBlob.UploadAsync(new BinaryData(prepared.Cover), new BlobUploadOptions { HttpHeaders = headers }, cancellationToken);
            _ = await thumbnailBlob.UploadAsync(new BinaryData(prepared.Thumbnail), new BlobUploadOptions { HttpHeaders = headers }, cancellationToken);

            itemRecord.HasCover = true;
            itemRecord.ModifiedDate = DateTimeOffset.UtcNow;
        }

        [HttpDelete("libraries/{libraryId}/items/{itemId}/cover")]
        public async Task<IActionResult> DeleteItemCoverAsync(long libraryId, long itemId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryItem itemRecord = await nsContext.LibraryItems.SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.Id == itemId, cancellationToken);
            if (itemRecord == default)
            {
                return this.NotFound($"Item [{itemId}] not found in library [{libraryId}].");
            }

            await this.DeleteCoverBlobsAsync([itemId], cancellationToken);
            itemRecord.HasCover = false;
            itemRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] removed the cover of item [{itemId}] in library [{libraryId}].");
            return this.NoContent();
        }

        /// <summary>
        /// Deletes the cover blobs of items whose rows are gone or going. Called after the rows are deleted, so a failure here
        /// leaves an unreferenced blob (logged) rather than an item pointing at a missing cover.
        /// </summary>
        private async Task DeleteCoverBlobsAsync(IReadOnlyCollection<long> itemIds, CancellationToken cancellationToken)
        {
            if (itemIds.Count == 0)
            {
                return;
            }
            NsStorage storage = this.serviceProvider.GetRequiredService<NsStorage>();
            int deleted = 0;
            foreach (long itemId in itemIds)
            {
                foreach (bool thumbnail in new[] { false, true })
                {
                    BlobClient blob = await storage.GetLibraryCoverBlobClientAsync(itemId, thumbnail, cancellationToken);
                    try
                    {
                        if (await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken))
                        {
                            deleted++;
                        }
                    }
                    catch (RequestFailedException ex)
                    {
                        this.logger.LogWarning($"Could not delete cover blob [{blob.Name}]: {ex.Message}");
                    }
                }
            }
            this.logger.LogInformation($"Deleted {deleted} cover blobs for {itemIds.Count} items.");
        }
    }
}
