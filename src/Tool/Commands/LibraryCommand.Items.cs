using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LibraryCommand
    {
        private static async Task HandleItemCreateAsync(long? libraryId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            LibraryItem item = new() { Title = null };
            PromptItemFields(item);
            item = await client.Library.CreateItemAsync(id, item);
            Console.WriteLine($"Created item [{item.Id}] [{item.Title}].");

            List<LibraryLocation> locations = null;
            while (Input.GetBool("Add a copy now?", defaultVal: true))
            {
                locations ??= await client.Library.GetLocationsAsync(id);
                LibraryCopy copy = await PromptAndCreateCopyAsync(client, id, item, locations);
                Console.WriteLine($"Added copy [{copy.Id}].");
            }

            await WriteItemAsync(client, id, item.Id);
        }

        private static async Task HandleItemGetAsync(long? libraryId, long itemId, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            if (raw)
            {
                LibraryItem item = await client.Library.GetItemAsync(id, itemId);
                Console.WriteLine(item.ToJsonString(indented: true));
            }
            else
            {
                await WriteItemAsync(client, id, itemId);
            }
        }

        private static async Task HandleItemEditAsync(long? libraryId, long itemId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            LibraryItem item = await client.Library.GetItemAsync(id, itemId);
            PromptItemFields(item);

            // The server ignores copies on an item update; no need to send them back.
            item.Copies = null;
            item = await client.Library.UpdateItemAsync(id, item);
            Console.WriteLine(item.ToJsonString(indented: true));
        }

        private static async Task HandleItemDeleteAsync(long? libraryId, long itemId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            LibraryItem item = await client.Library.GetItemAsync(id, itemId);
            if (!Input.GetBool($"Delete [{item.Title}] with its {item.Copies?.Count ?? 0} copies and all their loans and history? To keep the history of a copy that's gone, mark it {LibraryCopyStatus.Disposed} instead"))
            {
                Console.WriteLine("Not deleted.");
                return;
            }

            await client.Library.DeleteItemAsync(id, itemId);
            Console.WriteLine($"Deleted item [{itemId}] [{item.Title}].");
        }

        private static async Task HandleItemListAsync(
            long? libraryId,
            string query,
            LibraryMediaType? mediaType,
            long? locationId,
            bool loanedOut,
            int skip,
            int take,
            bool raw,
            Stamp stamp,
            Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            List<LibraryItem> items = await client.Library.GetItemsAsync(id, query, mediaType, locationId, loanedOut, skip, take);
            if (raw)
            {
                Console.WriteLine(items.ToJsonString(indented: true));
                return;
            }

            Console.WriteLine();
            if (items.Count == 0)
            {
                Console.WriteLine("No items found.");
                return;
            }

            Dictionary<long, string> paths = await GetLocationPathsAsync(client, id);
            Output.WriteTable(
                items.Select(x => new
                {
                    x.Id,
                    x.MediaType,
                    x.Title,
                    x.Creators,
                    x.Year,
                    Copies = $"{x.Copies.Count}: {string.Join("; ", x.Copies.Select(c => LocationPath(paths, c.LocationId)).Distinct())}",
                    OnLoanTo = string.Join("; ", x.Copies.SelectMany(c => c.Loans ?? []).Select(l => l.BorrowerName)),
                }),
                "Id",
                "MediaType",
                "Title",
                "Creators",
                "Year",
                "Copies",
                "OnLoanTo");
            if (items.Count == take)
            {
                Console.WriteLine($"Showing {take} from {skip}; there may be more (--skip {skip + take}).");
            }
        }

        private static void PromptItemFields(LibraryItem item)
        {
            item.MediaType = PromptEnum(nameof(item.MediaType), item.MediaType);
            item.Title = PromptRequiredString(nameof(item.Title), item.Title);
            item.Subtitle = PromptString(nameof(item.Subtitle), item.Subtitle);
            item.Creators = PromptString($"{nameof(item.Creators)} (semicolon-separated)", item.Creators);
            item.Series = PromptString(nameof(item.Series), item.Series);
            item.SeriesNumber = item.Series == null ? null : PromptValue(nameof(item.SeriesNumber), item.SeriesNumber);
            item.Year = PromptValue(nameof(item.Year), item.Year);
            item.Summary = PromptString(nameof(item.Summary), item.Summary);
            item.Tags = PromptString($"{nameof(item.Tags)} (semicolon-separated)", item.Tags);
        }

        /// <summary>
        /// The item, then a table of its copies, then the history of every copy in the order it happened.
        /// </summary>
        private static async Task WriteItemAsync(NsTcpWtfClient client, long libraryId, long itemId)
        {
            LibraryItem item = await client.Library.GetItemAsync(libraryId, itemId);
            Dictionary<long, string> paths = await GetLocationPathsAsync(client, libraryId);

            Console.WriteLine();
            Console.WriteLine($"#{item.Id} {item.MediaType}: {item.Title}{(item.Subtitle == null ? string.Empty : $": {item.Subtitle}")}");
            List<string> details = [];
            if (item.Creators != null)
            {
                details.Add(item.Creators);
            }
            if (item.Series != null)
            {
                details.Add($"{item.Series}{(item.SeriesNumber.HasValue ? $" #{item.SeriesNumber:0.##}" : string.Empty)}");
            }
            if (item.Year.HasValue)
            {
                details.Add(item.Year.ToString());
            }
            if (details.Count > 0)
            {
                Console.WriteLine(string.Join(" · ", details));
            }
            if (item.Tags != null)
            {
                Console.WriteLine($"Tags: {item.Tags}");
            }
            if (item.Summary != null)
            {
                Console.WriteLine();
                Console.WriteLine(item.Summary);
            }

            Console.WriteLine();
            if (item.Copies.Count == 0)
            {
                Console.WriteLine("No copies.");
                return;
            }
            Output.WriteTable(
                item.Copies.Select(c => new
                {
                    CopyId = c.Id,
                    c.Format,
                    c.Barcode,
                    Location = LocationPath(paths, c.LocationId),
                    c.Status,
                    OnLoanTo = c.Loans.Where(l => l.ReturnedDate == null).Select(l => l.DueDate.HasValue ? $"{l.BorrowerName}, due {l.DueDate:yyyy-MM-dd}" : l.BorrowerName).FirstOrDefault(),
                    Acquired = c.AcquiredDate?.ToString("yyyy-MM-dd"),
                }),
                "CopyId",
                "Format",
                "Barcode",
                "Location",
                "Status",
                "OnLoanTo",
                "Acquired");

            Dictionary<long, LibraryLoan> loans = item.Copies.SelectMany(c => c.Loans).ToDictionary(l => l.Id);
            Console.WriteLine();
            Output.WriteTable(
                item.Copies
                    .SelectMany(c => c.Events)
                    .OrderBy(e => e.OccurredDate)
                    .ThenBy(e => e.Id)
                    .Select(e => new
                    {
                        CopyId = e.CopyId,
                        When = e.OccurredDateIsDateOnly ? e.OccurredDate.ToString("yyyy-MM-dd") : e.OccurredDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                        Event = e.EventType,
                        Detail = DescribeEvent(e, paths, loans),
                        By = e.AccountName,
                    }),
                "CopyId",
                "When",
                "Event",
                "Detail",
                "By");
        }

        private static string DescribeEvent(LibraryCopyEvent copyEvent, Dictionary<long, string> paths, Dictionary<long, LibraryLoan> loans)
        {
            string detail = copyEvent.EventType switch
            {
                LibraryCopyEventType.Acquired => copyEvent.ToLocationId.HasValue ? $"at {LocationPath(paths, copyEvent.ToLocationId)}" : null,
                LibraryCopyEventType.Moved => $"{LocationPath(paths, copyEvent.FromLocationId)} → {LocationPath(paths, copyEvent.ToLocationId)}",
                LibraryCopyEventType.Loaned => copyEvent.LoanId.HasValue && loans.TryGetValue(copyEvent.LoanId.Value, out LibraryLoan loan) ? $"to {loan.BorrowerName}" : null,
                LibraryCopyEventType.Returned => copyEvent.LoanId.HasValue && loans.TryGetValue(copyEvent.LoanId.Value, out LibraryLoan loan) ? $"from {loan.BorrowerName}" : null,
                _ => null,
            };
            return string.Join(" — ", new[] { detail, copyEvent.Notes }.Where(x => x != null));
        }
    }
}
