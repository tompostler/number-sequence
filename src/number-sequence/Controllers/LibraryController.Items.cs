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
        /// <param name="sort">Shelf order unless asked otherwise; see <see cref="LibrarySort"/>.</param>
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
            [FromQuery] int take = 100,
            [FromQuery] LibraryItemSort sort = LibraryItemSort.Shelf)
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

            // The page's ids first. Shelf and title order need a last name and article-free titles, which sql can't do, so
            // every match's sort fields are read and sorted here; see LibrarySort.
            int matched;
            List<long> pageIds;
            if (sort == LibraryItemSort.Added)
            {
                pageIds = await query
                    .OrderByDescending(x => x.Copies.Max(c => (DateTimeOffset?)c.CreatedDate) ?? x.CreatedDate)
                    .ThenByDescending(x => x.Id)
                    .Skip(skip)
                    .Take(take)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);
                matched = -1;
            }
            else
            {
                List<LibraryItem> keys = await query
                    .Select(x => new LibraryItem
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Subtitle = x.Subtitle,
                        Creators = x.Creators,
                        Series = x.Series,
                        SeriesNumber = x.SeriesNumber,
                        Year = x.Year,
                    })
                    .ToListAsync(cancellationToken);
                matched = keys.Count;
                pageIds = LibrarySort.Order(keys, sort).Skip(skip).Take(take).Select(x => x.Id).ToList();
            }

            // The item is the only partial row in the response: summary, external ids and attributes are the large columns,
            // and a page can be 500 items. Copies and open loans come back whole and are attached to their item below.
            var rows = await nsContext.LibraryItems
                .AsNoTracking()
                .Where(x => pageIds.Contains(x.Id))
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

            Dictionary<long, int> position = pageIds.Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
            List<LibraryItem> items = [];
            foreach (var row in rows.OrderBy(x => position[x.Item.Id]))
            {
                foreach (LibraryCopy copy in row.Copies)
                {
                    copy.Loans = row.OpenLoans.Where(l => l.CopyId == copy.Id).ToList();
                }
                row.Item.Copies = row.Copies;
                items.Add(row.Item);
            }

            this.logger.LogInformation($"[{this.AccountName}] searched library [{libraryId}] q=[{q}] mediaType=[{mediaType}] locationId=[{locationId}] loanedOut=[{loanedOut}] externalId=[{externalIdKey}:{externalIdValue}] sort={sort} skip={skip} take={take}: {items.Count} items{(matched < 0 ? string.Empty : $" of {matched} sorted")} in {sw.ElapsedMilliseconds}ms.");
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

        /// <summary>
        /// Every series name in use in the library, for suggesting as one is typed and for matching a looked-up series to
        /// the spelling already used (<see cref="LibrarySeries.MatchExisting"/>).
        /// </summary>
        [HttpGet("libraries/{libraryId}/series")]
        public async Task<IActionResult> GetSeriesAsync(long libraryId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Viewer, cancellationToken);
            return this.Ok(await SeriesNamesAsync(nsContext, libraryId, cancellationToken));
        }

        /// <summary>
        /// Sets one series on several items, each with its own number: the bulk fix for a series a lookup misspelled or left
        /// out. The name is used as given, not matched to an existing spelling, since fixing a spelling is often the point.
        /// Every item must be in the library, or nothing changes. Returns the items as saved, in the order sent.
        /// </summary>
        [HttpPut("libraries/{libraryId}/series")]
        public async Task<IActionResult> SetSeriesAsync(long libraryId, [FromBody] LibrarySeriesAssign assign, CancellationToken cancellationToken)
        {
            const int maxItems = 500;
            string series = string.IsNullOrWhiteSpace(assign?.Series) ? null : assign.Series.Trim();
            List<LibrarySeriesAssignItem> entries = assign?.Items ?? [];
            if (entries.Count == 0 || entries.Count > maxItems)
            {
                return this.BadRequest($"Give between 1 and {maxItems} items.");
            }
            if (series?.Length > 128)
            {
                return this.BadRequest("A series name can be at most 128 characters.");
            }
            if (entries.Select(x => x.ItemId).Distinct().Count() != entries.Count)
            {
                return this.BadRequest("An item is listed more than once.");
            }
            // The column is decimal(8, 2).
            if (entries.FirstOrDefault(x => x.Number is < 0 or >= 1_000_000 || (x.Number.HasValue && decimal.Round(x.Number.Value, 2) != x.Number.Value)) is LibrarySeriesAssignItem bad)
            {
                return this.BadRequest($"[{bad.Number}] for item [{bad.ItemId}] isn't a number in a series: it must be between 0 and 999999.99, with at most two decimal places.");
            }

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            List<long> ids = entries.Select(x => x.ItemId).ToList();
            Dictionary<long, LibraryItem> records = await nsContext.LibraryItems
                .Where(x => x.LibraryId == libraryId && ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);
            List<long> missing = ids.Where(x => !records.ContainsKey(x)).ToList();
            if (missing.Count > 0)
            {
                return this.NotFound($"Item{(missing.Count == 1 ? string.Empty : "s")} [{string.Join(", ", missing)}] not found in library [{libraryId}].");
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            int changed = 0;
            foreach (LibrarySeriesAssignItem entry in entries)
            {
                LibraryItem record = records[entry.ItemId];
                decimal? number = series == null ? null : entry.Number;
                if (record.Series != series || record.SeriesNumber != number)
                {
                    record.Series = series;
                    record.SeriesNumber = number;
                    record.ModifiedDate = now;
                    changed++;
                }
            }
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] set series [{series}] on {entries.Count} items in library [{libraryId}], {changed} changed: {string.Join(", ", entries.Select(x => $"{x.ItemId}={x.Number}"))}.");
            return this.Ok(ids.Select(x => records[x]).ToList());
        }

        private static Task<List<string>> SeriesNamesAsync(NsContext nsContext, long libraryId, CancellationToken cancellationToken)
            => nsContext.LibraryItems
                .Where(x => x.LibraryId == libraryId && x.Series != null && x.Series != string.Empty)
                .Select(x => x.Series)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);
    }
}
