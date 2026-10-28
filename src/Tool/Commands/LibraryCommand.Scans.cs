using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;
using Unlimitedinf.Utilities;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    /// <summary>
    /// Scan sessions from a terminal, which suits a USB handheld scanner on a laptop: it types each code and Enter. Codes
    /// are read a line at a time until a blank line or end of input.
    /// </summary>
    internal static partial class LibraryCommand
    {
        private static async Task HandleScanListAsync(long? libraryId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);
            Dictionary<long, string> paths = await GetLocationPathsAsync(client, id);

            List<LibraryScanSession> sessions = await client.Library.GetScanSessionsAsync(id, open: false);
            Console.WriteLine();
            if (sessions.Count == 0)
            {
                Console.WriteLine("No scans yet. Start one with 'library scan start <locationId>'.");
                return;
            }
            Output.WriteTable(
                sessions.Select(x => new
                {
                    ScanId = x.Id,
                    Location = LocationPath(paths, x.LocationId),
                    Scanned = x.Entries.Count,
                    By = x.AccountName,
                    Started = x.CreatedDate.ToString("yyyy-MM-dd"),
                    Applied = x.CompletedDate?.ToString("yyyy-MM-dd") ?? "(open)",
                }),
                "ScanId",
                "Location",
                "Scanned",
                "By",
                "Started",
                "Applied");
        }

        private static async Task HandleScanStartAsync(long? libraryId, long locationId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            LibraryScanSession session = await client.Library.CreateScanSessionAsync(id, locationId);
            Console.WriteLine($"Started scan [{session.Id}]. Resume it later with 'library scan resume {session.Id}'.");
            await ScanLoopAsync(client, id, session);
        }

        private static async Task HandleScanResumeAsync(long? libraryId, long sessionId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            LibraryScanSession session = await client.Library.GetScanSessionAsync(id, sessionId);
            if (session.CompletedDate.HasValue)
            {
                throw new InvalidOperationException($"Scan [{sessionId}] was applied on {session.CompletedDate:yyyy-MM-dd}. Start a new one.");
            }
            Console.WriteLine($"Resuming scan [{session.Id}], {session.Entries.Count} scanned so far.");
            await ScanLoopAsync(client, id, session);
        }

        private static async Task ScanLoopAsync(NsTcpWtfClient client, long libraryId, LibraryScanSession session)
        {
            Dictionary<long, string> paths = await GetLocationPathsAsync(client, libraryId);
            Console.WriteLine($"Scanning {LocationPath(paths, session.LocationId)}. One code per line; a blank line ends.");
            Console.WriteLine();

            int count = 0;
            while (Console.ReadLine()?.Trim() is string line && line.Length > 0)
            {
                try
                {
                    LibraryScanEntry entry = await client.Library.AddScanEntryAsync(libraryId, session.Id, line);
                    Console.WriteLine($"  {DescribeScanEntry(entry, paths)}");
                    count++;
                }
                catch (NsTcpWtfClientException ex)
                {
                    Console.WriteLine($"  Refused: {ex.Message}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"{count} scanned. Review with 'library scan show {session.Id}', then 'library scan apply {session.Id}'.");
        }

        private static string DescribeScanEntry(LibraryScanEntry entry, Dictionary<long, string> paths)
        {
            string title = entry.ResolvedCopy?.Item?.Title ?? entry.Barcode;
            return entry.Resolution switch
            {
                LibraryScanResolution.Expected => $"Here       {title}",
                LibraryScanResolution.Elsewhere => $"Elsewhere  {title} (recorded at {LocationPath(paths, entry.ResolvedCopy?.LocationId)})",
                LibraryScanResolution.Added => $"Added      {title}",
                _ => $"New        {entry.Barcode}",
            };
        }

        private static async Task HandleScanShowAsync(long? libraryId, long sessionId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);
            Dictionary<long, string> paths = await GetLocationPathsAsync(client, id);
            LibraryScanReconcile reconcile = await client.Library.GetScanReconcileAsync(id, sessionId);
            WriteScanReconcile(reconcile, paths);
        }

        /// <summary>
        /// The reconcile view as tables, grouped the way the reconcile page groups them: by where each matched copy is now.
        /// </summary>
        private static void WriteScanReconcile(LibraryScanReconcile reconcile, Dictionary<long, string> paths)
        {
            LibraryScanSession session = reconcile.Session;
            Console.WriteLine();
            Console.WriteLine($"Scan [{session.Id}] of {LocationPath(paths, session.LocationId)} by {session.AccountName}, {session.Entries.Count} scanned{(session.CompletedDate.HasValue ? $", applied {session.CompletedDate:yyyy-MM-dd}" : string.Empty)}.");

            List<LibraryCopy> matched = session.Entries
                .Where(x => x.Resolution is LibraryScanResolution.Expected or LibraryScanResolution.Elsewhere && x.ResolvedCopy != null && x.ResolvedCopy.Status != LibraryCopyStatus.Disposed)
                .Select(x => x.ResolvedCopy)
                .ToList();
            WriteScanCopies("Here (verified on apply)", matched.Where(x => x.LocationId == session.LocationId), paths);
            WriteScanCopies("Recorded somewhere else (can be moved here)", matched.Where(x => x.LocationId != session.LocationId), paths);
            if (!session.CompletedDate.HasValue)
            {
                WriteScanCopies("Recorded here, not scanned (can be marked missing)", reconcile.NotScanned, paths);
            }

            List<LibraryScanEntry> news = session.Entries.Where(x => x.Resolution == LibraryScanResolution.NotInLibrary).ToList();
            if (news.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"New ({news.Count}), to add on the reconcile page or with 'library item create':");
                foreach (LibraryScanEntry entry in news)
                {
                    Console.WriteLine($"  {entry.Barcode}");
                }
            }
        }

        private static void WriteScanCopies(string heading, IEnumerable<LibraryCopy> copies, Dictionary<long, string> paths)
        {
            List<LibraryCopy> list = copies.ToList();
            if (list.Count == 0)
            {
                return;
            }
            Console.WriteLine();
            Console.WriteLine($"{heading} ({list.Count}):");
            Output.WriteTable(
                list.Select(x => new
                {
                    CopyId = x.Id,
                    x.Item?.Title,
                    x.Format,
                    Location = LocationPath(paths, x.LocationId),
                    x.Status,
                    OnLoanTo = x.Loans?.FirstOrDefault(l => l.ReturnedDate == null)?.BorrowerName,
                }),
                "CopyId",
                "Title",
                "Format",
                "Location",
                "Status",
                "OnLoanTo");
        }

        private static async Task HandleScanApplyAsync(long? libraryId, long sessionId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);
            Dictionary<long, string> paths = await GetLocationPathsAsync(client, id);
            LibraryScanReconcile reconcile = await client.Library.GetScanReconcileAsync(id, sessionId);
            WriteScanReconcile(reconcile, paths);
            Console.WriteLine();

            LibraryScanSession session = reconcile.Session;
            List<LibraryCopy> matched = session.Entries
                .Where(x => x.Resolution is LibraryScanResolution.Expected or LibraryScanResolution.Elsewhere && x.ResolvedCopy != null && x.ResolvedCopy.Status != LibraryCopyStatus.Disposed)
                .Select(x => x.ResolvedCopy)
                .ToList();
            List<long> elsewhere = matched.Where(x => x.LocationId != session.LocationId).Select(x => x.Id).ToList();
            List<long> onLoan = matched.Where(x => x.Loans?.Any(l => l.ReturnedDate == null) == true).Select(x => x.Id).ToList();
            List<long> missable = reconcile.NotScanned.Where(x => x.Loans?.Any(l => l.ReturnedDate == null) != true).Select(x => x.Id).ToList();

            LibraryScanApply apply = new()
            {
                MoveCopyIds = elsewhere.Count > 0 && Input.GetBool($"Move the {elsewhere.Count} copies recorded somewhere else here?", defaultVal: true) ? elsewhere : [],
                ReturnCopyIds = onLoan.Count > 0 && Input.GetBool($"Mark the {onLoan.Count} scanned copies on loan as returned?", defaultVal: true) ? onLoan : [],
                MissingCopyIds = missable.Count > 0 && Input.GetBool($"Mark the {missable.Count} copies recorded here but not scanned as missing?", defaultVal: false) ? missable : [],
            };
            if (!Input.GetBool($"Apply scan [{sessionId}]?", defaultVal: true))
            {
                return;
            }

            _ = await client.Library.ApplyScanSessionAsync(id, sessionId, apply);
            Console.WriteLine($"Applied: {matched.Count - elsewhere.Count} verified, {apply.MoveCopyIds.Count} moved, {apply.ReturnCopyIds.Count} returned, {apply.MissingCopyIds.Count} marked missing.");
        }

        private static async Task HandleScanDeleteAsync(long? libraryId, long sessionId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);
            if (!Input.GetBool($"Delete scan [{sessionId}]? Nothing it would have applied is applied, and copies added from it stay"))
            {
                return;
            }
            await client.Library.DeleteScanSessionAsync(id, sessionId);
            Console.WriteLine($"Deleted scan [{sessionId}].");
        }
    }
}
