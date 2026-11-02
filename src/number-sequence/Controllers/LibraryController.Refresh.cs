using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Exceptions;
using number_sequence.Services.Libraries;
using number_sequence.Utilities;
using System.Globalization;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Controllers
{
    /// <summary>
    /// Looking a saved item up again and taking chosen fields from it, for items added before the lookup improved and for
    /// an Open Library record fixed since. Here rather than in the page so the page and the Tool compare the same way.
    /// </summary>
    public sealed partial class LibraryController
    {
        [HttpGet("libraries/{libraryId}/items/{itemId}/refresh")]
        public async Task<IActionResult> GetItemRefreshAsync(long libraryId, long itemId, CancellationToken cancellationToken, [FromQuery] long? copyId = null)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryItem item = await GetItemWithCopiesAsync(nsContext, libraryId, itemId, cancellationToken);
            Refresh refresh = await this.BuildRefreshAsync(nsContext, item, copyId, cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] compared item [{itemId}] in library [{libraryId}] with {refresh.View.LookedUpBy ?? "nothing"}: {refresh.View.Changes.Count} changes{(refresh.View.Note == null ? string.Empty : $", {refresh.View.Note}")}.");
            return this.Ok(refresh.View);
        }

        /// <summary>
        /// Takes the chosen fields. The lookup is made again (an hour's cache makes that the same answer as the comparison)
        /// for the values that aren't sent: attributes, external ids and cover.
        /// </summary>
        [HttpPost("libraries/{libraryId}/items/{itemId}/refresh")]
        public async Task<IActionResult> ApplyItemRefreshAsync(long libraryId, long itemId, [FromBody] LibraryItemRefreshApply apply, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryItem item = await GetItemWithCopiesAsync(nsContext, libraryId, itemId, cancellationToken, tracked: true);
            Refresh refresh = await this.BuildRefreshAsync(nsContext, item, apply?.CopyId, cancellationToken);
            if (refresh.Result == null)
            {
                throw new ConflictException(refresh.View.Note ?? "The lookup has nothing for this item.");
            }

            LibraryLookupResult result = refresh.Result;
            LibraryCopy copy = refresh.Copy;
            LibraryItemRefreshApplied applied = new();
            bool itemChanged = false;
            bool copyChanged = false;
            bool takeCover = false;
            foreach ((string field, string value) in apply?.Take ?? [])
            {
                string label = Labels.GetValueOrDefault(field) ?? throw new BadRequestException($"[{field}] isn't a field a refresh can take.");
                switch (field)
                {
                    case LibraryItemRefreshFields.Title:
                        item.Title = Text(value, 256, label) ?? throw new BadRequestException("The title can't be blank.");
                        break;
                    case LibraryItemRefreshFields.Subtitle:
                        item.Subtitle = Text(value, 256, label);
                        break;
                    case LibraryItemRefreshFields.Creators:
                        item.Creators = Text(value, 512, label);
                        break;
                    case LibraryItemRefreshFields.Series:
                        item.Series = Text(value, 128, label);
                        break;
                    case LibraryItemRefreshFields.SeriesNumber:
                        item.SeriesNumber = Number<decimal>(value, label);
                        break;
                    case LibraryItemRefreshFields.Year:
                        item.Year = Number<int>(value, label);
                        break;
                    case LibraryItemRefreshFields.Summary:
                        item.Summary = Text(value, LookupText.MaxSummaryLength, label);
                        break;
                    case LibraryItemRefreshFields.Attributes:
                        item.Attributes = Merge(item.Attributes, result.Attributes);
                        break;
                    case LibraryItemRefreshFields.ExternalIds:
                        item.ExternalIds = Merge(item.ExternalIds, result.ExternalIds);
                        break;
                    case LibraryItemRefreshFields.Cover:
                        takeCover = true;
                        continue;
                    default:
                        if (copy == null)
                        {
                            throw new BadRequestException($"{label} comes from a copy's ISBN, and this item wasn't looked up by one.");
                        }
                        switch (field)
                        {
                            case LibraryItemRefreshFields.Format:
                                copy.Format = Text(value, 32, label);
                                break;
                            case LibraryItemRefreshFields.Publisher:
                                copy.Publisher = Text(value, 128, label);
                                break;
                            case LibraryItemRefreshFields.PublishedYear:
                                copy.PublishedYear = Number<int>(value, label);
                                break;
                        }
                        copyChanged = true;
                        applied.Taken.Add(label);
                        continue;
                }
                itemChanged = true;
                applied.Taken.Add(label);
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (itemChanged)
            {
                item.ModifiedDate = now;
            }
            if (copyChanged)
            {
                copy.ModifiedDate = now;
            }
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            if (takeCover)
            {
                applied.CoverNote = result.CoverUrls?.Count > 0 ? null : "The lookup has no cover.";
                if (applied.CoverNote == null)
                {
                    try
                    {
                        LibraryCovers.Prepared prepared = await this.serviceProvider.GetRequiredService<LibraryCovers>().FetchAndPrepareAsync(result.CoverUrls, cancellationToken);
                        if (prepared == null)
                        {
                            applied.CoverNote = "The cover image couldn't be fetched.";
                        }
                        else
                        {
                            await this.StoreCoverAsync(item, prepared, cancellationToken);
                            _ = await nsContext.SaveChangesAsync(cancellationToken);
                            applied.Taken.Add(Labels[LibraryItemRefreshFields.Cover]);
                        }
                    }
                    catch (RequestFailedException ex)
                    {
                        applied.CoverNote = $"The cover couldn't be stored: {ex.Message}";
                    }
                }
            }

            this.logger.LogInformation($"[{this.AccountName}] refreshed item [{itemId}] in library [{libraryId}] from {result.Source} [{result.SourceId}] by {refresh.View.LookedUpBy}: took [{string.Join(", ", applied.Taken)}]{(applied.CoverNote == null ? string.Empty : $", {applied.CoverNote}")}.");
            return this.Ok(applied);
        }

        private static readonly Dictionary<string, string> Labels = new()
        {
            [LibraryItemRefreshFields.Title] = "title",
            [LibraryItemRefreshFields.Subtitle] = "subtitle",
            [LibraryItemRefreshFields.Creators] = "creators",
            [LibraryItemRefreshFields.Series] = "series",
            [LibraryItemRefreshFields.SeriesNumber] = "number in series",
            [LibraryItemRefreshFields.Year] = "year",
            [LibraryItemRefreshFields.Summary] = "summary",
            [LibraryItemRefreshFields.Attributes] = "details",
            [LibraryItemRefreshFields.ExternalIds] = "lookup ids",
            [LibraryItemRefreshFields.Cover] = "cover",
            [LibraryItemRefreshFields.Format] = "copy format",
            [LibraryItemRefreshFields.Publisher] = "copy publisher",
            [LibraryItemRefreshFields.PublishedYear] = "copy published year",
        };

        private sealed record Refresh(LibraryItemRefresh View, LibraryLookupResult Result, LibraryCopy Copy);

        private static async Task<LibraryItem> GetItemWithCopiesAsync(NsContext nsContext, long libraryId, long itemId, CancellationToken cancellationToken, bool tracked = false)
        {
            IQueryable<LibraryItem> query = nsContext.LibraryItems.Include(x => x.Copies);
            return await (tracked ? query : query.AsNoTracking()).SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.Id == itemId, cancellationToken)
                ?? throw new NotFoundException($"Item [{itemId}] not found in library [{libraryId}].");
        }

        /// <summary>
        /// Looks <paramref name="item"/> up, by a copy's ISBN when it's a book with one (<paramref name="copyId"/>'s, else
        /// the first), else by the first lookup id it was added with, and compares. A field the lookup has nothing for is
        /// never offered: taking it would only blank what's saved. The looked-up series is respelled to the library's.
        /// </summary>
        private async Task<Refresh> BuildRefreshAsync(NsContext nsContext, LibraryItem item, long? copyId, CancellationToken cancellationToken)
        {
            LibraryItemRefresh view = new() { ItemId = item.Id };
            List<(LibraryCopy Copy, string Isbn)> isbnCopies = item.MediaType != LibraryMediaType.Book
                ? []
                : item.Copies
                    .Where(x => x.Status != LibraryCopyStatus.Disposed)
                    .OrderBy(x => x.Id)
                    .Select(x => (Copy: x, Isbn: CopyIsbn(x)))
                    .Where(x => x.Isbn != null)
                    .ToList();
            view.IsbnCopyIds = isbnCopies.Select(x => x.Copy.Id).ToList();
            (LibraryCopy copy, string isbn) = copyId.HasValue ? isbnCopies.FirstOrDefault(x => x.Copy.Id == copyId.Value) : isbnCopies.FirstOrDefault();
            if (copyId.HasValue && copy == null)
            {
                throw new BadRequestException($"Copy [{copyId}] isn't a copy of this item with an ISBN.");
            }

            LibraryLookup lookup = this.serviceProvider.GetRequiredService<LibraryLookup>();
            LibraryLookupResult result;
            if (copy != null)
            {
                view.CopyId = copy.Id;
                view.LookedUpBy = $"ISBN {isbn}";
                result = await lookup.LookupIsbnAsync(isbn, cancellationToken);
                view.Note = result == null ? $"No book lookup source knows ISBN {isbn}." : null;
            }
            else
            {
                Dictionary<string, string> ids = item.ExternalIds ?? [];
                (string source, string key, string name) = new[]
                {
                    (LibraryLookupSources.OpenLibrary, LibraryExternalIds.OpenLibraryWork, "Open Library work"),
                    (LibraryLookupSources.TmdbMovie, LibraryExternalIds.TmdbMovie, "TMDB movie"),
                    (LibraryLookupSources.TmdbTv, LibraryExternalIds.TmdbTv, "TMDB series"),
                    (LibraryLookupSources.BoardGameGeek, LibraryExternalIds.BoardGameGeek, "BoardGameGeek game"),
                }.FirstOrDefault(x => !string.IsNullOrWhiteSpace(ids.GetValueOrDefault(x.Item2)));
                if (source == null)
                {
                    view.Note = "There's nothing to look this up by: no copy has an ISBN, and it wasn't added from a lookup.";
                    return new(view, null, null);
                }
                view.LookedUpBy = $"{name} {ids[key]}";
                result = await lookup.GetAsync(source, ids[key], cancellationToken);
                view.Note = result == null ? $"There's no {name} {ids[key]} any more." : null;
            }
            if (result == null)
            {
                return new(view, null, copy);
            }

            // Not written back onto the result: it's the cached object, shared with every library's lookups.
            string series = LibrarySeries.MatchExisting(result.Series, await SeriesNamesAsync(nsContext, item.LibraryId, cancellationToken));
            view.Source = result.Source;
            view.SourceUrl = result.SourceUrl;
            view.CoverUrls = result.CoverUrls ?? [];

            void Compare(string field, string current, string offered)
            {
                if (string.IsNullOrWhiteSpace(offered))
                {
                    return;
                }
                string label = Labels[field];
                if (string.Equals(current?.Trim(), offered.Trim(), StringComparison.Ordinal))
                {
                    view.Same.Add(label);
                    return;
                }
                view.Changes.Add(new() { Field = field, Label = label, Current = string.IsNullOrWhiteSpace(current) ? null : current, Offered = offered });
            }

            Compare(LibraryItemRefreshFields.Title, item.Title, result.Title);
            Compare(LibraryItemRefreshFields.Subtitle, item.Subtitle, result.Subtitle);
            Compare(LibraryItemRefreshFields.Creators, item.Creators, result.Creators);
            Compare(LibraryItemRefreshFields.Series, item.Series, series);
            Compare(LibraryItemRefreshFields.SeriesNumber, Format(item.SeriesNumber), Format(result.SeriesNumber));
            Compare(LibraryItemRefreshFields.Year, Format(item.Year), Format(result.Year));
            Compare(LibraryItemRefreshFields.Summary, item.Summary, result.Summary);
            Compare(LibraryItemRefreshFields.Attributes, Describe(item.Attributes), Describe(Merge(item.Attributes, result.Attributes)));
            Compare(LibraryItemRefreshFields.ExternalIds, Describe(item.ExternalIds), Describe(Merge(item.ExternalIds, result.ExternalIds)));
            if (copy != null)
            {
                Compare(LibraryItemRefreshFields.Format, copy.Format, result.Format);
                Compare(LibraryItemRefreshFields.Publisher, copy.Publisher, result.Publisher);
                Compare(LibraryItemRefreshFields.PublishedYear, Format(copy.PublishedYear), Format(result.PublishedYear));
            }
            return new(view, result, copy);
        }

        private static string CopyIsbn(LibraryCopy copy)
            => copy.Isbn ?? (LibraryBarcode.TryParse(copy.Barcode, out LibraryBarcode barcode, out _) && barcode.Kind == LibraryBarcodeKind.Isbn ? barcode.Value : null);

        /// <summary>
        /// What's saved with the lookup's entries added or replacing, so a key only the saved item has is kept.
        /// </summary>
        private static Dictionary<string, string> Merge(Dictionary<string, string> saved, Dictionary<string, string> fetched)
        {
            Dictionary<string, string> merged = new(saved ?? []);
            foreach ((string key, string value) in fetched ?? [])
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    merged[key] = value;
                }
            }
            return merged;
        }

        private static string Describe(Dictionary<string, string> values)
            => values == null || values.Count == 0 ? null : string.Join(" · ", values.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => $"{x.Key} {x.Value}"));

        private static string Format(decimal? number) => number?.ToString("0.##", CultureInfo.InvariantCulture);

        private static string Format(int? number) => number?.ToString(CultureInfo.InvariantCulture);

        private static string Text(string value, int maxLength, string label)
        {
            string trimmed = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            return trimmed?.Length > maxLength ? throw new BadRequestException($"The {label} is longer than {maxLength} characters.") : trimmed;
        }

        private static T? Number<T>(string value, string label) where T : struct, IParsable<T>
            => string.IsNullOrWhiteSpace(value) ? null
                : T.TryParse(value.Trim(), CultureInfo.InvariantCulture, out T number) ? number
                : throw new BadRequestException($"[{value}] isn't a number, for the {label}.");
    }
}
