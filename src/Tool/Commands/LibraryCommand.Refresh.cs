using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;
using Unlimitedinf.Utilities;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    /// <summary>
    /// Refresh from lookup for one item or a whole library, the way the item page's Refresh does it (the comparison is the
    /// api's), with a prompt per item. For a library added before the lookup improved, or after fixing records on Open
    /// Library.
    /// </summary>
    internal static partial class LibraryCommand
    {
        private const int RefreshColumnWidth = 60;

        private static async Task HandleItemRefreshAsync(long? libraryId, long? itemId, bool all, LibraryMediaType? mediaType, long? copyId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            if (itemId.HasValue)
            {
                LibraryItem item = await client.Library.GetItemAsync(id, itemId.Value);
                _ = await RefreshItemAsync(client, id, item, copyId, quietWhenSame: false);
                return;
            }
            if (!all)
            {
                throw new ArgumentException("Give an item id, or --all for every item in the library.");
            }
            if (copyId.HasValue)
            {
                throw new ArgumentException("--copy is for one item; it can't go with --all.");
            }

            List<LibraryItem> items = [];
            const int page = 500;
            for (int skip = 0; ; skip += page)
            {
                List<LibraryItem> batch = await client.Library.GetItemsAsync(id, mediaType: mediaType, skip: skip, take: page);
                items.AddRange(batch);
                if (batch.Count < page)
                {
                    break;
                }
            }
            Console.WriteLine($"Comparing {items.Count} items with a fresh lookup.");

            int changed = 0;
            // The api's shelf order, so a series comes up together and in order.
            foreach (LibraryItem item in items)
            {
                if (await RefreshItemAsync(client, id, item, copyId: null, quietWhenSame: true))
                {
                    changed++;
                }
            }
            Console.WriteLine();
            Console.WriteLine($"Done: {changed} of {items.Count} items changed.");
        }

        /// <summary>
        /// Shows one item's comparison and asks what to take. True when something was taken.
        /// </summary>
        private static async Task<bool> RefreshItemAsync(NsTcpWtfClient client, long libraryId, LibraryItem item, long? copyId, bool quietWhenSame)
        {
            LibraryItemRefresh refresh;
            try
            {
                refresh = await client.Library.GetItemRefreshAsync(libraryId, item.Id, copyId);
            }
            catch (NsTcpWtfClientException ex)
            {
                // One provider failing shouldn't end a run through the whole library.
                Console.WriteLine();
                Console.WriteLine($"[{item.Id}] {item.Title}: lookup didn't work. {ex.Message}");
                return false;
            }

            bool coverOffered = refresh.Source != null && refresh.CoverUrls.Count > 0;
            bool nothingNew = refresh.Note != null || (refresh.Changes.Count == 0 && (!coverOffered || item.HasCover));
            if (nothingNew && quietWhenSame)
            {
                if (refresh.Note != null)
                {
                    Console.WriteLine($"[{item.Id}] {item.Title}: {refresh.Note}");
                }
                return false;
            }

            Console.WriteLine();
            Console.WriteLine($"[{item.Id}] {item.Title}, looked up by {refresh.LookedUpBy ?? "nothing"}{(refresh.SourceUrl == null ? string.Empty : $" ({refresh.SourceUrl})")}");
            if (refresh.IsbnCopyIds.Count > 1)
            {
                Console.WriteLine($"  Copy [{refresh.CopyId}]'s edition; other copies with an ISBN: {string.Join(", ", refresh.IsbnCopyIds.Where(x => x != refresh.CopyId))} (use --copy).");
            }
            if (refresh.Note != null)
            {
                Console.WriteLine($"  {refresh.Note}");
                return false;
            }
            if (refresh.Changes.Count == 0 && !coverOffered)
            {
                Console.WriteLine("  Everything the lookup has already matches.");
                return false;
            }

            List<(string Field, string Label, string Value)> offered = refresh.Changes.Select(x => (x.Field, x.Label, x.Offered)).ToList();
            if (coverOffered)
            {
                offered.Add((LibraryItemRefreshFields.Cover, item.HasCover ? "cover (replacing the saved one)" : "cover", null));
            }
            Output.WriteTable(
                refresh.Changes.Select(x => new { Field = x.Label, Now = Shorten(x.Current ?? "(empty)"), Lookup = Shorten(x.Offered) })
                    .Concat(Enumerable.Repeat(new { Field = "cover", Now = item.HasCover ? "(saved)" : "(none)", Lookup = coverOffered ? Shorten(refresh.CoverUrls[^1]) : null }, coverOffered ? 1 : 0)),
                "Field",
                "Now",
                "Lookup");
            if (refresh.Same.Count > 0)
            {
                Console.WriteLine($"  Already the same: {string.Join(", ", refresh.Same)}.");
            }

            // Taking everything never replaces a saved cover: whether the lookup's is any better can't be seen from here.
            List<(string Field, string Label, string Value)> everything = offered.Where(x => x.Field != LibraryItemRefreshFields.Cover || !item.HasCover).ToList();
            List<(string Field, string Label, string Value)> take;
            if (everything.Count > 0 && Input.GetBool($"Take all {everything.Count}{(everything.Count < offered.Count ? ", keeping the saved cover" : string.Empty)}? No to choose one at a time", defaultVal: false))
            {
                take = everything;
            }
            else
            {
                take = offered.Where(x => Input.GetBool($"  Take the {x.Label}?", defaultVal: x.Field != LibraryItemRefreshFields.Cover || !item.HasCover)).ToList();
            }
            if (take.Count == 0)
            {
                Console.WriteLine("  Left as it was.");
                return false;
            }

            LibraryItemRefreshApplied applied = await client.Library.ApplyItemRefreshAsync(
                libraryId,
                item.Id,
                new LibraryItemRefreshApply { CopyId = refresh.CopyId, Take = take.ToDictionary(x => x.Field, x => x.Value) });
            Console.WriteLine($"  Took the {string.Join(", ", applied.Taken)}.{(applied.CoverNote == null ? string.Empty : $" {applied.CoverNote}")}");
            return applied.Taken.Count > 0;
        }

        private static string Shorten(string value)
        {
            string line = value?.ReplaceLineEndings(" ");
            return line == null || line.Length <= RefreshColumnWidth ? line : line[..(RefreshColumnWidth - 1)] + "…";
        }
    }
}
