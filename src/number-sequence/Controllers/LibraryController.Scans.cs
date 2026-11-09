using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Exceptions;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Controllers
{
    /// <summary>
    /// Scan sessions: one location, scanned shelf by shelf, then reconciled. Entries are saved as they're scanned, so a phone
    /// that locks mid-shelf loses nothing, and each is matched to a copy then and there. Applying the session is the only
    /// step that changes copies, and it does all of it in one save.
    /// </summary>
    public sealed partial class LibraryController
    {
        private const int ClosedScanSessionsListed = 20;

        /// <summary>
        /// Open sessions, newest first, or with <paramref name="open"/> false the newest few of any kind. Each comes with its
        /// entries, but not their copies.
        /// </summary>
        [HttpGet("libraries/{libraryId}/scans")]
        public async Task<IActionResult> GetScanSessionsAsync(long libraryId, CancellationToken cancellationToken, [FromQuery] bool open = true)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Viewer, cancellationToken);
            IQueryable<LibraryScanSession> query = nsContext.LibraryScanSessions
                .AsNoTracking()
                .Include(x => x.Entries)
                .Where(x => x.LibraryId == libraryId);
            query = open
                ? query.Where(x => x.CompletedDate == null).OrderByDescending(x => x.CreatedDate)
                : query.OrderByDescending(x => x.CreatedDate).Take(ClosedScanSessionsListed);
            List<LibraryScanSession> sessions = await query.ToListAsync(cancellationToken);
            return this.Ok(sessions);
        }

        /// <summary>
        /// Starts a session at <see cref="LibraryScanSession.LocationId"/>. Another open session at the same location is
        /// allowed; the scan page lists open ones to resume.
        /// </summary>
        [HttpPost("libraries/{libraryId}/scans")]
        public async Task<IActionResult> CreateScanSessionAsync(long libraryId, [FromBody] LibraryScanSession session, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            await EnsureLocationInLibraryAsync(nsContext, libraryId, session.LocationId, cancellationToken);

            LibraryScanSession toInsert = new()
            {
                LibraryId = libraryId,
                LocationId = session.LocationId,
                AccountName = this.AccountName,
            };
            _ = nsContext.LibraryScanSessions.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            toInsert.Entries = [];

            this.logger.LogInformation($"[{this.AccountName}] started scan [{toInsert.Id}] at location [{toInsert.LocationId}] in library [{libraryId}].");
            return this.Ok(toInsert);
        }

        /// <summary>
        /// The session with its entries in scan order, each with its <see cref="LibraryScanEntry.ResolvedCopy"/>.
        /// </summary>
        [HttpGet("libraries/{libraryId}/scans/{sessionId}")]
        public async Task<IActionResult> GetScanSessionAsync(long libraryId, long sessionId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Viewer, cancellationToken);
            LibraryScanSession session = await GetScanSessionAsync(nsContext, libraryId, sessionId, mustBeOpen: false, cancellationToken);
            await FillResolvedCopiesAsync(nsContext, libraryId, session.Entries, cancellationToken);
            return this.Ok(session);
        }

        /// <summary>
        /// Abandons a session, or forgets an applied one. Copies added from it stay; nothing it applied is undone.
        /// </summary>
        [HttpDelete("libraries/{libraryId}/scans/{sessionId}")]
        public async Task<IActionResult> DeleteScanSessionAsync(long libraryId, long sessionId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            int deleted = await nsContext.LibraryScanSessions.Where(x => x.LibraryId == libraryId && x.Id == sessionId).ExecuteDeleteAsync(cancellationToken);
            if (deleted == 0)
            {
                return this.NotFound($"Scan [{sessionId}] not found in library [{libraryId}].");
            }

            this.logger.LogInformation($"[{this.AccountName}] deleted scan [{sessionId}] in library [{libraryId}].");
            return this.NoContent();
        }

        /// <summary>
        /// Records one scanned code, matched to a copy as described on <see cref="Resolve"/>. Returns the entry with its
        /// <see cref="LibraryScanEntry.ResolvedCopy"/>, which is what the scan page shows for it.
        /// </summary>
        [HttpPost("libraries/{libraryId}/scans/{sessionId}/entries")]
        public async Task<IActionResult> AddScanEntryAsync(long libraryId, long sessionId, [FromBody] LibraryScanEntry entry, CancellationToken cancellationToken)
        {
            string barcode = NormalizeBarcodeOrThrow(entry?.Barcode) ?? throw new BadRequestException("Barcode is empty.");

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryScanSession session = await GetScanSessionAsync(nsContext, libraryId, sessionId, mustBeOpen: true, cancellationToken);

            List<LibraryCopy> candidates = await GetScanCandidatesAsync(nsContext, libraryId, LibraryBarcode.MatchesCopy(barcode), cancellationToken);
            HashSet<long> claimed = session.Entries.Where(x => x.ResolvedCopyId.HasValue).Select(x => x.ResolvedCopyId.Value).ToHashSet();
            (LibraryScanResolution resolution, long? copyId) = Resolve(candidates, barcode, session.LocationId, claimed);

            LibraryScanEntry toInsert = new()
            {
                SessionId = sessionId,
                Barcode = barcode,
                Resolution = resolution,
                ResolvedCopyId = copyId,
            };
            _ = nsContext.LibraryScanEntries.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            await FillResolvedCopiesAsync(nsContext, libraryId, [toInsert], cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] scanned [{entry.Barcode}] as [{barcode}] in scan [{sessionId}]: {resolution} copy [{copyId}] ({candidates.Count} candidates, {claimed.Count} already claimed).");
            return this.Ok(toInsert);
        }

        /// <summary>
        /// Removes an accidental scan. The removed entry may have claimed the copy a later scan of the same code missed, so
        /// every remaining entry is matched again in scan order, except those already added from the reconcile view.
        /// </summary>
        [HttpDelete("libraries/{libraryId}/scans/{sessionId}/entries/{entryId}")]
        public async Task<IActionResult> DeleteScanEntryAsync(long libraryId, long sessionId, long entryId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryScanSession session = await GetScanSessionAsync(nsContext, libraryId, sessionId, mustBeOpen: true, cancellationToken);
            LibraryScanEntry entryRecord = session.Entries.SingleOrDefault(x => x.Id == entryId);
            if (entryRecord == default)
            {
                return this.NotFound($"Entry [{entryId}] not found in scan [{sessionId}].");
            }
            _ = nsContext.LibraryScanEntries.Remove(entryRecord);

            List<LibraryScanEntry> remaining = session.Entries.Where(x => x.Id != entryId).OrderBy(x => x.CreatedDate).ThenBy(x => x.Id).ToList();
            HashSet<long> claimed = remaining.Where(x => x.Resolution == LibraryScanResolution.Added && x.ResolvedCopyId.HasValue).Select(x => x.ResolvedCopyId.Value).ToHashSet();
            List<string> codes = remaining.Select(x => x.Barcode).Distinct().ToList();
            List<LibraryCopy> candidates = codes.Count == 0 ? [] : await GetScanCandidatesAsync(nsContext, libraryId, LibraryBarcode.MatchesAnyCopy(codes), cancellationToken);
            int changed = 0;
            foreach (LibraryScanEntry other in remaining.Where(x => x.Resolution != LibraryScanResolution.Added))
            {
                (LibraryScanResolution resolution, long? copyId) = Resolve(candidates, other.Barcode, session.LocationId, claimed);
                if (resolution != other.Resolution || copyId != other.ResolvedCopyId)
                {
                    other.Resolution = resolution;
                    other.ResolvedCopyId = copyId;
                    changed++;
                }
            }
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] removed entry [{entryId}] [{entryRecord.Barcode}] from scan [{sessionId}]; {changed} other entries matched differently.");
            return this.NoContent();
        }

        /// <summary>
        /// Marks a <see cref="LibraryScanResolution.NotInLibrary"/> entry <see cref="LibraryScanResolution.Added"/> as copy
        /// <paramref name="copyId"/>, after that copy was added from the add page. The copy is in the route rather than an
        /// entry body, since an entry's barcode is required and the caller has no reason to send it.
        /// </summary>
        [HttpPost("libraries/{libraryId}/scans/{sessionId}/entries/{entryId}/resolve/{copyId}")]
        public async Task<IActionResult> ResolveScanEntryAsync(long libraryId, long sessionId, long entryId, long copyId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryScanSession session = await GetScanSessionAsync(nsContext, libraryId, sessionId, mustBeOpen: true, cancellationToken);
            LibraryScanEntry entryRecord = session.Entries.SingleOrDefault(x => x.Id == entryId)
                ?? throw new NotFoundException($"Entry [{entryId}] not found in scan [{sessionId}].");
            if (entryRecord.Resolution != LibraryScanResolution.NotInLibrary)
            {
                return this.Conflict($"Entry [{entryId}] already matched copy [{entryRecord.ResolvedCopyId}].");
            }
            if (!await nsContext.LibraryCopies.AnyAsync(x => x.LibraryId == libraryId && x.Id == copyId && x.Status != LibraryCopyStatus.Disposed, cancellationToken))
            {
                return this.BadRequest($"Copy [{copyId}] is not in library [{libraryId}].");
            }
            if (session.Entries.Any(x => x.ResolvedCopyId == copyId))
            {
                return this.Conflict($"Copy [{copyId}] already matched another entry in scan [{sessionId}].");
            }

            entryRecord.Resolution = LibraryScanResolution.Added;
            entryRecord.ResolvedCopyId = copyId;
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            await FillResolvedCopiesAsync(nsContext, libraryId, [entryRecord], cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] resolved entry [{entryId}] [{entryRecord.Barcode}] of scan [{sessionId}] to added copy [{copyId}].");
            return this.Ok(entryRecord);
        }

        [HttpGet("libraries/{libraryId}/scans/{sessionId}/reconcile")]
        public async Task<IActionResult> GetScanReconcileAsync(long libraryId, long sessionId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Viewer, cancellationToken);
            LibraryScanSession session = await GetScanSessionAsync(nsContext, libraryId, sessionId, mustBeOpen: false, cancellationToken);
            await FillResolvedCopiesAsync(nsContext, libraryId, session.Entries, cancellationToken);

            List<long> claimed = session.Entries.Where(x => x.ResolvedCopyId.HasValue).Select(x => x.ResolvedCopyId.Value).ToList();
            List<LibraryCopy> notScanned = await nsContext.LibraryCopies
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Item)
                .Include(x => x.Loans.Where(l => l.ReturnedDate == null))
                .Where(x => x.LibraryId == libraryId && x.LocationId == session.LocationId && x.Status == LibraryCopyStatus.Active && !claimed.Contains(x.Id))
                .OrderBy(x => x.Item.Title)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            return this.Ok(new LibraryScanReconcile { Session = session, NotScanned = notScanned });
        }

        /// <summary>
        /// Applies the session in one save, and closes it:
        /// <list type="bullet">
        /// <item>a matched copy that was <see cref="LibraryCopyStatus.Missing"/> is restored, since it was found;</item>
        /// <item>a matched copy in <see cref="LibraryScanApply.ReturnCopyIds"/> has its open loan returned today;</item>
        /// <item>a matched copy recorded here is verified, and one in <see cref="LibraryScanApply.MoveCopyIds"/> is moved here;</item>
        /// <item>copies in <see cref="LibraryScanApply.MissingCopyIds"/> are marked missing.</item>
        /// </list>
        /// Copies are read as they are now, not as they were when scanned: one disposed since is left alone, and one moved
        /// here since is verified rather than moved.
        /// </summary>
        [HttpPost("libraries/{libraryId}/scans/{sessionId}/apply")]
        public async Task<IActionResult> ApplyScanSessionAsync(long libraryId, long sessionId, [FromBody] LibraryScanApply apply, CancellationToken cancellationToken)
        {
            apply ??= new LibraryScanApply();
            apply.MoveCopyIds ??= [];
            apply.ReturnCopyIds ??= [];
            apply.MissingCopyIds ??= [];

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryScanSession session = await GetScanSessionAsync(nsContext, libraryId, sessionId, mustBeOpen: true, cancellationToken);

            HashSet<long> matched = session.Entries
                .Where(x => x.ResolvedCopyId.HasValue && x.Resolution is LibraryScanResolution.Expected or LibraryScanResolution.Elsewhere)
                .Select(x => x.ResolvedCopyId.Value)
                .ToHashSet();
            HashSet<long> claimed = session.Entries.Where(x => x.ResolvedCopyId.HasValue).Select(x => x.ResolvedCopyId.Value).ToHashSet();
            if (apply.MoveCopyIds.Concat(apply.ReturnCopyIds).FirstOrDefault(x => !matched.Contains(x)) is long unmatched and > 0)
            {
                return this.BadRequest($"Copy [{unmatched}] wasn't matched by a scan in this session, so it can't be moved or returned here.");
            }

            List<long> matchedIds = matched.ToList();
            List<LibraryCopy> matchedCopies = await nsContext.LibraryCopies
                .Include(x => x.Loans.Where(l => l.ReturnedDate == null))
                .Where(x => x.LibraryId == libraryId && matchedIds.Contains(x.Id))
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);
            List<long> missingIds = apply.MissingCopyIds.Distinct().ToList();
            List<LibraryCopy> missingCopies = await nsContext.LibraryCopies
                .Where(x => x.LibraryId == libraryId && missingIds.Contains(x.Id))
                .Where(x => x.LocationId == session.LocationId && x.Status == LibraryCopyStatus.Active && !x.Loans.Any(l => l.ReturnedDate == null))
                .ToListAsync(cancellationToken);
            if (missingCopies.Count != missingIds.Count || missingCopies.Any(x => claimed.Contains(x.Id)))
            {
                return this.BadRequest("Only active copies recorded here, not on loan and not scanned, can be marked missing. Reload the reconcile view; something changed.");
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            int verified = 0, moved = 0, restored = 0, returned = 0;
            foreach (LibraryCopy copy in matchedCopies)
            {
                if (copy.Status == LibraryCopyStatus.Disposed)
                {
                    continue;
                }
                if (copy.Status == LibraryCopyStatus.Missing)
                {
                    copy.Status = LibraryCopyStatus.Active;
                    _ = nsContext.LibraryCopyEvents.Add(this.NewScanEvent(LibraryCopyEventType.Restored, copy.Id));
                    copy.ModifiedDate = now;
                    restored++;
                }
                if (copy.Loans.SingleOrDefault() is LibraryLoan openLoan && apply.ReturnCopyIds.Contains(copy.Id))
                {
                    openLoan.ReturnedDate = TodayUtc < openLoan.LoanedDate ? openLoan.LoanedDate : TodayUtc;
                    openLoan.ModifiedDate = now;
                    LibraryCopyEvent returnedEvent = this.NewScanEvent(LibraryCopyEventType.Returned, copy.Id);
                    returnedEvent.LoanId = openLoan.Id;
                    _ = nsContext.LibraryCopyEvents.Add(returnedEvent);
                    returned++;
                }
                if (copy.LocationId == session.LocationId)
                {
                    _ = nsContext.LibraryCopyEvents.Add(this.NewScanEvent(LibraryCopyEventType.Verified, copy.Id));
                    verified++;
                }
                else if (apply.MoveCopyIds.Contains(copy.Id))
                {
                    LibraryCopyEvent movedEvent = this.NewScanEvent(LibraryCopyEventType.Moved, copy.Id);
                    movedEvent.FromLocationId = copy.LocationId;
                    movedEvent.ToLocationId = session.LocationId;
                    _ = nsContext.LibraryCopyEvents.Add(movedEvent);
                    copy.LocationId = session.LocationId;
                    copy.ModifiedDate = now;
                    moved++;
                }
            }
            foreach (LibraryCopy copy in missingCopies)
            {
                copy.Status = LibraryCopyStatus.Missing;
                copy.ModifiedDate = now;
                _ = nsContext.LibraryCopyEvents.Add(this.NewScanEvent(LibraryCopyEventType.MarkedMissing, copy.Id));
            }
            session.CompletedDate = now;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] applied scan [{sessionId}] at location [{session.LocationId}] in library [{libraryId}] with {session.Entries.Count} entries: {verified} verified, {moved} moved, {restored} restored, {returned} returned, {missingCopies.Count} marked missing.");
            return this.Ok(session);
        }

        private LibraryCopyEvent NewScanEvent(LibraryCopyEventType eventType, long copyId)
        {
            LibraryCopyEvent copyEvent = this.NewEvent(eventType);
            copyEvent.CopyId = copyId;
            return copyEvent;
        }

        /// <summary>
        /// The session, tracked, with its entries. A 404 when it isn't in the library, and with <paramref name="mustBeOpen"/>
        /// a 409 once it has been applied.
        /// </summary>
        private static async Task<LibraryScanSession> GetScanSessionAsync(NsContext nsContext, long libraryId, long sessionId, bool mustBeOpen, CancellationToken cancellationToken)
        {
            LibraryScanSession session = await nsContext.LibraryScanSessions
                .Include(x => x.Entries.OrderBy(e => e.CreatedDate).ThenBy(e => e.Id))
                .SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.Id == sessionId, cancellationToken)
                ?? throw new NotFoundException($"Scan [{sessionId}] not found in library [{libraryId}].");
            if (mustBeOpen && session.CompletedDate.HasValue)
            {
                throw new ConflictException($"Scan [{sessionId}] was applied on {session.CompletedDate:yyyy-MM-dd}. Start a new one.");
            }
            return session;
        }

        /// <summary>
        /// The copies a scan could match: not disposed, and identified by a code the way every other code lookup does it.
        /// Disposed copies are left out on purpose; one that turns up again is restored from its item page.
        /// </summary>
        private static Task<List<LibraryCopy>> GetScanCandidatesAsync(NsContext nsContext, long libraryId, System.Linq.Expressions.Expression<Func<LibraryCopy, bool>> matches, CancellationToken cancellationToken)
            => nsContext.LibraryCopies
                .AsNoTracking()
                .Where(matches)
                .Where(x => x.LibraryId == libraryId && x.Status != LibraryCopyStatus.Disposed)
                .ToListAsync(cancellationToken);

        /// <summary>
        /// Matches one scanned code to at most one copy among <paramref name="candidates"/> that an earlier entry hasn't
        /// <paramref name="claimed"/>, and claims it. A copy recorded at the session's location is preferred, so scanning a
        /// shelf never reports a move for a copy that was already there; then any other, lowest id first. None left is
        /// <see cref="LibraryScanResolution.NotInLibrary"/>: a third scan of a paperback you own two of is a copy you didn't
        /// know you had.
        /// </summary>
        private static (LibraryScanResolution Resolution, long? CopyId) Resolve(List<LibraryCopy> candidates, string code, long locationId, HashSet<long> claimed)
        {
            List<LibraryCopy> unclaimed = candidates.AsQueryable().Where(LibraryBarcode.MatchesCopy(code)).Where(x => !claimed.Contains(x.Id)).OrderBy(x => x.Id).ToList();
            LibraryCopy match = unclaimed.FirstOrDefault(x => x.LocationId == locationId) ?? unclaimed.FirstOrDefault();
            if (match == null)
            {
                return (LibraryScanResolution.NotInLibrary, null);
            }
            _ = claimed.Add(match.Id);
            return (match.LocationId == locationId ? LibraryScanResolution.Expected : LibraryScanResolution.Elsewhere, match.Id);
        }

        /// <summary>
        /// Fills in <see cref="LibraryScanEntry.ResolvedCopy"/> with each copy as it is now, with its item and open loan.
        /// </summary>
        private static async Task FillResolvedCopiesAsync(NsContext nsContext, long libraryId, ICollection<LibraryScanEntry> entries, CancellationToken cancellationToken)
        {
            List<long> copyIds = entries.Where(x => x.ResolvedCopyId.HasValue).Select(x => x.ResolvedCopyId.Value).Distinct().ToList();
            if (copyIds.Count == 0)
            {
                return;
            }
            Dictionary<long, LibraryCopy> copies = await nsContext.LibraryCopies
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Item)
                .Include(x => x.Loans.Where(l => l.ReturnedDate == null))
                .Where(x => x.LibraryId == libraryId && copyIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);
            foreach (LibraryScanEntry entry in entries)
            {
                entry.ResolvedCopy = entry.ResolvedCopyId.HasValue ? copies.GetValueOrDefault(entry.ResolvedCopyId.Value) : null;
            }
        }
    }
}
