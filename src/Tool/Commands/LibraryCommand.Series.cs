using System.Globalization;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    /// <summary>
    /// The bulk series fix, as on the library's Series page: one name for several items, each with its own number.
    /// </summary>
    internal static partial class LibraryCommand
    {
        private const string LeaveOut = "x";

        private static async Task HandleSeriesListAsync(long? libraryId, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            List<string> names = await client.Library.GetSeriesAsync(id);
            if (raw)
            {
                Console.WriteLine(names.ToJsonString(indented: true));
                return;
            }
            Console.WriteLine();
            if (names.Count == 0)
            {
                Console.WriteLine("No series yet.");
                return;
            }
            names.ForEach(Console.WriteLine);
        }

        /// <summary>
        /// Either the items named on the command line (<c>id</c> keeps its number, <c>id=n</c> sets one), or every match of
        /// <paramref name="query"/> with a prompt per item.
        /// </summary>
        private static async Task HandleSeriesSetAsync(long? libraryId, string series, List<string> itemArgs, string query, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);
            string name = series == Clear ? null : series;

            bool hasItems = itemArgs?.Count > 0;
            bool hasQuery = !string.IsNullOrWhiteSpace(query);
            if (hasItems == hasQuery)
            {
                throw new ArgumentException("Give item ids (id, or id=number), or --query to choose from the matches, but not both.");
            }

            List<LibrarySeriesAssignItem> entries = hasItems
                ? await EntriesFromArgumentsAsync(client, id, itemArgs)
                : await EntriesFromPromptsAsync(client, id, query.Trim(), name);
            if (entries.Count == 0)
            {
                Console.WriteLine("Nothing chosen, so nothing changed.");
                return;
            }

            List<LibraryItem> saved = await client.Library.SetSeriesAsync(id, new LibrarySeriesAssign { Series = name, Items = entries });
            Console.WriteLine();
            Output.WriteTable(
                saved.Select(x => new { x.Id, x.Title, x.Series, Number = x.SeriesNumber?.ToString("0.##", CultureInfo.InvariantCulture), x.Year }),
                "Id",
                "Title",
                "Series",
                "Number",
                "Year");
            Console.WriteLine(name == null ? $"Took {saved.Count} out of their series." : $"Put {saved.Count} in {name}.");
        }

        private static async Task<List<LibrarySeriesAssignItem>> EntriesFromArgumentsAsync(NsTcpWtfClient client, long libraryId, List<string> itemArgs)
        {
            List<LibrarySeriesAssignItem> entries = [];
            foreach (string arg in itemArgs)
            {
                string[] parts = arg.Split('=', 2, StringSplitOptions.TrimEntries);
                if (!long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out long itemId))
                {
                    throw new ArgumentException($"[{arg}] isn't an item id, or an id=number.");
                }
                decimal? number;
                if (parts.Length == 2)
                {
                    number = string.IsNullOrEmpty(parts[1]) || parts[1] == Clear
                        ? null
                        : decimal.TryParse(parts[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal parsed)
                            ? parsed
                            : throw new ArgumentException($"[{parts[1]}] in [{arg}] isn't a number.");
                }
                else
                {
                    // A bare id keeps its number, which the api would otherwise clear.
                    number = (await client.Library.GetItemAsync(libraryId, itemId)).SeriesNumber;
                }
                entries.Add(new() { ItemId = itemId, Number = number });
            }
            return entries;
        }

        private static async Task<List<LibrarySeriesAssignItem>> EntriesFromPromptsAsync(NsTcpWtfClient client, long libraryId, string query, string name)
        {
            List<LibraryItem> found = await client.Library.GetItemsAsync(libraryId, query, take: 500);
            if (found.Count == 0)
            {
                Console.WriteLine($"Nothing matches [{query}].");
                return [];
            }

            // Likely series order, as on the Series page: numbered by number, then the rest by year.
            List<LibraryItem> items = found
                .OrderBy(x => !x.SeriesNumber.HasValue)
                .ThenBy(x => x.SeriesNumber)
                .ThenBy(x => !x.Year.HasValue)
                .ThenBy(x => x.Year)
                .ThenBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            Console.WriteLine();
            Output.WriteTable(
                items.Select(x => new { x.Id, x.Title, x.Creators, x.Series, Number = x.SeriesNumber?.ToString("0.##", CultureInfo.InvariantCulture), x.Year }),
                "Id",
                "Title",
                "Creators",
                "Series",
                "Number",
                "Year");
            Console.WriteLine($"Putting these in {name ?? "no series"}. For each: a number, blank to keep the one shown, {Clear} for none, {LeaveOut} to leave it out.");

            List<LibrarySeriesAssignItem> entries = [];
            foreach (LibraryItem item in items)
            {
                string current = item.SeriesNumber?.ToString("0.##", CultureInfo.InvariantCulture);
                while (true)
                {
                    Console.Write($"  [{item.Id}] {item.Title}{(current == null ? string.Empty : $" (default {current})")}: ");
                    string input = Console.ReadLine()?.Trim();
                    if (string.Equals(input, LeaveOut, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    if (string.IsNullOrEmpty(input) || input == Clear)
                    {
                        entries.Add(new() { ItemId = item.Id, Number = string.IsNullOrEmpty(input) ? item.SeriesNumber : null });
                        break;
                    }
                    if (decimal.TryParse(input, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal number))
                    {
                        entries.Add(new() { ItemId = item.Id, Number = number });
                        break;
                    }
                    Console.WriteLine($"  [{input}] isn't a number.");
                }
            }

            return entries.Count > 0 && Input.GetBool($"Set {name ?? "no series"} on {entries.Count} item{(entries.Count == 1 ? string.Empty : "s")}?", defaultVal: true)
                ? entries
                : [];
        }
    }
}
