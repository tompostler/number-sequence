using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Exceptions;
using number_sequence.Utilities;
using System.Diagnostics;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Controllers
{
    public sealed partial class LibraryController
    {
        /// <summary>
        /// Search and browse. Each item comes without <see cref="LibraryItem.Summary"/>, <see cref="LibraryItem.ExternalIds"/>
        /// and <see cref="LibraryItem.Attributes"/> (null here even when set), with every copy whole and each copy's
        /// <see cref="LibraryCopy.Loans"/> holding only its open loan. <see cref="GetItemAsync"/> has the rest.
        /// </summary>
        /// <param name="q">Matched against title, subtitle, creators and series. If it parses as a barcode, items with a copy
        /// carrying that barcode match too, so a scanned code can be pasted into the same search box.</param>
        /// <param name="locationId">Items with a copy at this location or anywhere inside it.</param>
        /// <param name="loanedOut">Only items with a copy currently on loan.</param>
        /// <param name="externalIdKey">With <paramref name="externalIdValue"/>, only items whose
        /// <see cref="LibraryItem.ExternalIds"/> has that entry. How a lookup result finds the item it's already in the
        /// library as.</param>
        [HttpGet("libraries/{libraryId}/items")]
        public async Task<IActionResult> GetItemsAsync(
            long libraryId,
            CancellationToken cancellationToken,
            [FromQuery] string q = null,
            [FromQuery] LibraryMediaType? mediaType = null,
            [FromQuery] long? locationId = null,
            [FromQuery] bool loanedOut = false,
            [FromQuery] string externalIdKey = null,
            [FromQuery] string externalIdValue = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100)
        {
            if (take < 1 || take > 500)
            {
                return this.BadRequest("Take must be between 1 and 500.");
            }

            Stopwatch sw = Stopwatch.StartNew();
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Viewer, cancellationToken);

            IQueryable<LibraryItem> query = nsContext.LibraryItems.AsNoTracking().Where(x => x.LibraryId == libraryId);
            if (!string.IsNullOrWhiteSpace(q))
            {
                string term = q.Trim();
                string barcode = LibraryBarcode.TryParse(term, out LibraryBarcode parsed, out _) ? parsed.Value : null;
                query = query.Where(x =>
                    x.Title.Contains(term)
                    || x.Subtitle.Contains(term)
                    || x.Creators.Contains(term)
                    || x.Series.Contains(term)
                    || (barcode != null && x.Copies.AsQueryable().Any(LibraryBarcode.MatchesCopy(barcode))));
            }
            if (mediaType.HasValue)
            {
                query = query.Where(x => x.MediaType == mediaType.Value);
            }
            if (locationId.HasValue)
            {
                List<long> locationIds = await GetLocationAndDescendantIdsAsync(nsContext, libraryId, locationId.Value, cancellationToken);
                query = query.Where(x => x.Copies.Any(c => c.LocationId.HasValue && locationIds.Contains(c.LocationId.Value)));
            }
            if (loanedOut)
            {
                query = query.Where(x => x.Copies.Any(c => c.Loans.Any(l => l.ReturnedDate == null)));
            }
            if (!string.IsNullOrWhiteSpace(externalIdKey) && !string.IsNullOrWhiteSpace(externalIdValue))
            {
                List<long> matching = await ItemIdsByExternalIdAsync(nsContext, libraryId, externalIdKey, externalIdValue, cancellationToken);
                query = query.Where(x => matching.Contains(x.Id));
            }

            // The item is the only partial row in the response: summary, external ids and attributes are the large columns,
            // and a page can be 500 items. Copies and open loans come back whole and are attached to their item below.
            var rows = await query
                .OrderBy(x => x.Title)
                .ThenBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    Item = new LibraryItem
                    {
                        Id = x.Id,
                        LibraryId = x.LibraryId,
                        MediaType = x.MediaType,
                        Title = x.Title,
                        Subtitle = x.Subtitle,
                        Creators = x.Creators,
                        Series = x.Series,
                        SeriesNumber = x.SeriesNumber,
                        Year = x.Year,
                        Tags = x.Tags,
                        HasCover = x.HasCover,
                        CreatedDate = x.CreatedDate,
                        ModifiedDate = x.ModifiedDate,
                    },
                    Copies = x.Copies.OrderBy(c => c.Id).ToList(),
                    OpenLoans = x.Copies.SelectMany(c => c.Loans).Where(l => l.ReturnedDate == null).ToList(),
                })
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            List<LibraryItem> items = [];
            foreach (var row in rows)
            {
                foreach (LibraryCopy copy in row.Copies)
                {
                    copy.Loans = row.OpenLoans.Where(l => l.CopyId == copy.Id).ToList();
                }
                row.Item.Copies = row.Copies;
                items.Add(row.Item);
            }

            this.logger.LogInformation($"[{this.AccountName}] searched library [{libraryId}] q=[{q}] mediaType=[{mediaType}] locationId=[{locationId}] loanedOut=[{loanedOut}] externalId=[{externalIdKey}:{externalIdValue}] skip={skip} take={take}: {items.Count} items in {sw.ElapsedMilliseconds}ms.");
            return this.Ok(items);
        }

        /// <summary>
        /// One item with every copy, each copy's loans (newest first) and its history in the order it happened, with
        /// <see cref="LibraryCopyEvent.OccurredDate"/> filled in by <see cref="LibraryEventTimes"/>.
        /// </summary>
        [HttpGet("libraries/{libraryId}/items/{itemId}")]
        public async Task<IActionResult> GetItemAsync(long libraryId, long itemId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Viewer, cancellationToken);
            LibraryItem item = await nsContext.LibraryItems
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Copies)
                    .ThenInclude(x => x.Loans)
                .Include(x => x.Copies)
                    .ThenInclude(x => x.Events)
                .SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.Id == itemId, cancellationToken);
            if (item == default)
            {
                return this.NotFound($"Item [{itemId}] not found in library [{libraryId}].");
            }

            item.Copies = item.Copies.OrderBy(x => x.Id).ToList();
            foreach (LibraryCopy copy in item.Copies)
            {
                List<LibraryEventTimes.Timed> timed = LibraryEventTimes.Compute(copy, copy.Events, copy.Loans);
                foreach (LibraryEventTimes.Timed t in timed)
                {
                    t.Event.OccurredDate = t.Time;
                    t.Event.OccurredDateIsDateOnly = t.IsDateOnly;
                }
                copy.Events = timed.Select(x => x.Event).ToList();
                copy.Loans = copy.Loans.OrderByDescending(x => x.LoanedDate).ThenByDescending(x => x.Id).ToList();
            }

            return this.Ok(item);
        }

        [HttpPost("libraries/{libraryId}/items")]
        public async Task<IActionResult> CreateItemAsync(long libraryId, [FromBody] LibraryItem item, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);

            LibraryItem toInsert = new() { LibraryId = libraryId };
            CopyEditableFields(item, toInsert);
            _ = nsContext.LibraryItems.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] created {toInsert.MediaType} item [{toInsert.Id}] [{toInsert.Title}] in library [{libraryId}].");
            return this.Ok(toInsert);
        }

        [HttpPut("libraries/{libraryId}/items")]
        public async Task<IActionResult> UpdateItemAsync(long libraryId, [FromBody] LibraryItem item, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryItem itemRecord = await nsContext.LibraryItems.SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.Id == item.Id, cancellationToken);
            if (itemRecord == default)
            {
                return this.NotFound($"Item [{item.Id}] not found in library [{libraryId}].");
            }

            CopyEditableFields(item, itemRecord);
            itemRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] updated item [{itemRecord.Id}] [{itemRecord.Title}] in library [{libraryId}].");
            return this.Ok(itemRecord);
        }

        /// <summary>
        /// Deletes the item and, by cascade, its copies with their loans and history. Disposing a copy keeps its history;
        /// this is for an item that shouldn't have been created.
        /// </summary>
        [HttpDelete("libraries/{libraryId}/items/{itemId}")]
        public async Task<IActionResult> DeleteItemAsync(long libraryId, long itemId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            int deleted = await nsContext.LibraryItems.Where(x => x.LibraryId == libraryId && x.Id == itemId).ExecuteDeleteAsync(cancellationToken);
            if (deleted == 0)
            {
                return this.NotFound($"Item [{itemId}] not found in library [{libraryId}].");
            }

            await this.DeleteCoverBlobsAsync([itemId], cancellationToken);
            this.logger.LogInformation($"[{this.AccountName}] deleted item [{itemId}] in library [{libraryId}].");
            return this.NoContent();
        }

        /// <summary>
        /// The ids of items in <paramref name="libraryId"/> whose external id <paramref name="key"/> is <paramref name="value"/>.
        /// The column is json, which EF only sees as the converted dictionary, so the match is sql. The key becomes a json
        /// path, so it is limited to the plain identifiers <see cref="LibraryExternalIds"/> uses.
        /// </summary>
        private static async Task<List<long>> ItemIdsByExternalIdAsync(NsContext nsContext, long libraryId, string key, string value, CancellationToken cancellationToken)
        {
            if (!key.All(char.IsAsciiLetterOrDigit))
            {
                throw new BadRequestException("An external id key is letters and digits only.");
            }
            string path = $"$.{key}";
            return await nsContext.Database
                .SqlQuery<long>($"SELECT Id AS Value FROM dbo.LibraryItems WHERE LibraryId = {libraryId} AND JSON_VALUE(ExternalIds, {path}) = {value}")
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Everything a caller may set on an item. <see cref="LibraryItem.HasCover"/> is left out because it follows the
        /// stored cover, not the request.
        /// </summary>
        private static void CopyEditableFields(LibraryItem from, LibraryItem to)
        {
            to.MediaType = from.MediaType;
            to.Title = from.Title;
            to.Subtitle = from.Subtitle;
            to.Creators = from.Creators;
            to.Series = from.Series;
            to.SeriesNumber = from.SeriesNumber;
            to.Year = from.Year;
            to.Summary = from.Summary;
            to.Tags = from.Tags;
            to.ExternalIds = from.ExternalIds;
            to.Attributes = from.Attributes;
        }
    }
}
