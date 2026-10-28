using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Exceptions;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Controllers
{
    /// <summary>
    /// A copy's location, status and loans only change through the dedicated actions below (move, status, loan, return),
    /// because each one writes a <see cref="LibraryCopyEvent"/>. The general update leaves all three alone.
    /// </summary>
    public sealed partial class LibraryController
    {
        [HttpGet("libraries/{libraryId}/copies/{copyId}")]
        public async Task<IActionResult> GetCopyAsync(long libraryId, long copyId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Viewer, cancellationToken);
            LibraryCopy copy = await nsContext.LibraryCopies
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.Id == copyId, cancellationToken);
            return copy == default ? this.NotFound($"Copy [{copyId}] not found in library [{libraryId}].") : this.Ok(copy);
        }

        /// <summary>
        /// Adds a copy of an existing item, active, and records its <see cref="LibraryCopyEventType.Acquired"/> event. The
        /// copy limit is the owner's tier, not the caller's, since an editor adds to someone else's library.
        /// </summary>
        [HttpPost("libraries/{libraryId}/copies")]
        public async Task<IActionResult> CreateCopyAsync(long libraryId, [FromBody] LibraryCopy copy, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Library library = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            if (!await nsContext.LibraryItems.AnyAsync(x => x.Id == copy.ItemId && x.LibraryId == libraryId, cancellationToken))
            {
                return this.BadRequest($"Item [{copy.ItemId}] is not in library [{libraryId}].");
            }

            LibraryCopy toInsert = await this.AddNewCopyAsync(nsContext, library, copy, cancellationToken);
            toInsert.ItemId = copy.ItemId;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] created copy [{toInsert.Id}] of item [{toInsert.ItemId}] at location [{toInsert.LocationId}] with barcode [{toInsert.Barcode}] in library [{libraryId}].");
            return this.Ok(toInsert);
        }

        /// <summary>
        /// Updates a copy's descriptive fields. Changing <see cref="LibraryCopy.ItemId"/> to another item in the same library
        /// is allowed, which is how a copy added as a separate item gets merged into the right one.
        /// </summary>
        [HttpPut("libraries/{libraryId}/copies")]
        public async Task<IActionResult> UpdateCopyAsync(long libraryId, [FromBody] LibraryCopy copy, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryCopy copyRecord = await nsContext.LibraryCopies.SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.Id == copy.Id, cancellationToken);
            if (copyRecord == default)
            {
                return this.NotFound($"Copy [{copy.Id}] not found in library [{libraryId}].");
            }
            if (copy.ItemId != copyRecord.ItemId && !await nsContext.LibraryItems.AnyAsync(x => x.Id == copy.ItemId && x.LibraryId == libraryId, cancellationToken))
            {
                return this.BadRequest($"Item [{copy.ItemId}] is not in library [{libraryId}].");
            }

            long previousItemId = copyRecord.ItemId;
            copyRecord.ItemId = copy.ItemId;
            CopyEditableFields(copy, copyRecord);
            copyRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] updated copy [{copyRecord.Id}] (item [{previousItemId}] -> [{copyRecord.ItemId}]) in library [{libraryId}].");
            return this.Ok(copyRecord);
        }

        /// <summary>
        /// Deletes the copy with its loans and history. For a copy that is gone but should stay in history, set its status to
        /// <see cref="LibraryCopyStatus.Disposed"/> instead.
        /// </summary>
        [HttpDelete("libraries/{libraryId}/copies/{copyId}")]
        public async Task<IActionResult> DeleteCopyAsync(long libraryId, long copyId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            int deleted = await nsContext.LibraryCopies.Where(x => x.LibraryId == libraryId && x.Id == copyId).ExecuteDeleteAsync(cancellationToken);
            if (deleted == 0)
            {
                return this.NotFound($"Copy [{copyId}] not found in library [{libraryId}].");
            }

            this.logger.LogInformation($"[{this.AccountName}] deleted copy [{copyId}] in library [{libraryId}].");
            return this.NoContent();
        }

        /// <summary>
        /// Moving to where the copy already is records nothing. A disposed copy can't be moved; restore it first.
        /// </summary>
        [HttpPost("libraries/{libraryId}/copies/{copyId}/move")]
        public async Task<IActionResult> MoveCopyAsync(long libraryId, long copyId, [FromBody] LibraryCopyMove move, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryCopy copyRecord = await nsContext.LibraryCopies.SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.Id == copyId, cancellationToken);
            if (copyRecord == default)
            {
                return this.NotFound($"Copy [{copyId}] not found in library [{libraryId}].");
            }
            if (copyRecord.Status == LibraryCopyStatus.Disposed)
            {
                return this.Conflict($"Copy [{copyId}] is disposed. Restore it before moving it.");
            }
            await EnsureLocationInLibraryAsync(nsContext, libraryId, move.LocationId, cancellationToken);
            if (copyRecord.LocationId == move.LocationId)
            {
                return this.Ok(copyRecord);
            }

            LibraryCopyEvent moved = this.NewEvent(LibraryCopyEventType.Moved, move.Notes);
            moved.CopyId = copyId;
            moved.FromLocationId = copyRecord.LocationId;
            moved.ToLocationId = move.LocationId;
            _ = nsContext.LibraryCopyEvents.Add(moved);
            copyRecord.LocationId = move.LocationId;
            copyRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] moved copy [{copyId}] from [{moved.FromLocationId}] to [{moved.ToLocationId}] in library [{libraryId}].");
            return this.Ok(copyRecord);
        }

        /// <summary>
        /// Changes a copy's status, recording <see cref="LibraryCopyEventType.MarkedMissing"/>,
        /// <see cref="LibraryCopyEventType.Disposed"/> or <see cref="LibraryCopyEventType.Restored"/>. Setting the status it
        /// already has records nothing. A copy on loan has to be returned first: the borrower has it, so it is neither
        /// missing nor gone.
        /// </summary>
        [HttpPost("libraries/{libraryId}/copies/{copyId}/status")]
        public async Task<IActionResult> SetCopyStatusAsync(long libraryId, long copyId, [FromBody] LibraryCopyStatusChange change, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryCopy copyRecord = await nsContext.LibraryCopies.SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.Id == copyId, cancellationToken);
            if (copyRecord == default)
            {
                return this.NotFound($"Copy [{copyId}] not found in library [{libraryId}].");
            }
            if (copyRecord.Status == change.Status)
            {
                return this.Ok(copyRecord);
            }
            if (change.Status != LibraryCopyStatus.Active && await nsContext.LibraryLoans.AnyAsync(x => x.CopyId == copyId && x.ReturnedDate == null, cancellationToken))
            {
                return this.Conflict($"Copy [{copyId}] is on loan. Return it before marking it {change.Status}.");
            }

            LibraryCopyEvent statusEvent = this.NewEvent(
                change.Status switch
                {
                    LibraryCopyStatus.Missing => LibraryCopyEventType.MarkedMissing,
                    LibraryCopyStatus.Disposed => LibraryCopyEventType.Disposed,
                    _ => LibraryCopyEventType.Restored,
                },
                change.Notes);
            statusEvent.CopyId = copyId;
            _ = nsContext.LibraryCopyEvents.Add(statusEvent);

            LibraryCopyStatus previous = copyRecord.Status;
            copyRecord.Status = change.Status;
            copyRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] changed copy [{copyId}] from {previous} to {copyRecord.Status} in library [{libraryId}].");
            return this.Ok(copyRecord);
        }

        /// <summary>
        /// Lends an active copy that isn't already out. The loan's id is drawn from its sequence up front: the
        /// <see cref="LibraryCopyEvent.LoanId"/> that has to reference it is a plain column with no foreign key, so EF can't
        /// fill it in on insert, and drawing the id first keeps the loan and its event in one save without a transaction.
        /// </summary>
        [HttpPost("libraries/{libraryId}/copies/{copyId}/loan")]
        public async Task<IActionResult> LoanCopyAsync(long libraryId, long copyId, [FromBody] LibraryLoan loan, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            LibraryCopy copyRecord = await nsContext.LibraryCopies.SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.Id == copyId, cancellationToken);
            if (copyRecord == default)
            {
                return this.NotFound($"Copy [{copyId}] not found in library [{libraryId}].");
            }
            if (copyRecord.Status != LibraryCopyStatus.Active)
            {
                return this.Conflict($"Copy [{copyId}] is {copyRecord.Status}, so it can't be lent.");
            }
            if (await nsContext.LibraryLoans.AnyAsync(x => x.CopyId == copyId && x.ReturnedDate == null, cancellationToken))
            {
                return this.Conflict($"Copy [{copyId}] is already on loan.");
            }
            DateOnly loanedDate = loan.LoanedDate == default ? TodayUtc : loan.LoanedDate;
            if (loan.DueDate.HasValue && loan.DueDate < loanedDate)
            {
                return this.BadRequest("The due date is before the loan date.");
            }

            // Not composed (no SingleAsync/FirstAsync): EF would wrap the sql in SELECT TOP(n) ... FROM (...), and sql server
            // refuses NEXT VALUE FOR in both a derived table and a TOP statement.
            List<long> loanIds = await nsContext.Database
                .SqlQuery<long>($"SELECT NEXT VALUE FOR dbo.LibraryLoanIds AS [Value]")
                .ToListAsync(cancellationToken);
            LibraryLoan toInsert = new()
            {
                Id = loanIds.Single(),
                CopyId = copyId,
                BorrowerName = loan.BorrowerName,
                BorrowerAccountName = await EnsureAccountExistsAsync(nsContext, loan.BorrowerAccountName, cancellationToken),
                LoanedDate = loanedDate,
                DueDate = loan.DueDate,
                Notes = loan.Notes,
            };
            LibraryCopyEvent loaned = this.NewEvent(LibraryCopyEventType.Loaned);
            loaned.CopyId = copyId;
            loaned.LoanId = toInsert.Id;
            _ = nsContext.LibraryLoans.Add(toInsert);
            _ = nsContext.LibraryCopyEvents.Add(loaned);
            try
            {
                _ = await nsContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 })
            {
                // IX_LibraryLoans_CopyId_Open: another request lent this copy between the check above and this insert.
                return this.Conflict($"Copy [{copyId}] is already on loan.");
            }

            this.logger.LogInformation($"[{this.AccountName}] lent copy [{copyId}] to [{toInsert.BorrowerName}] (account [{toInsert.BorrowerAccountName}]) as loan [{toInsert.Id}] on {toInsert.LoanedDate:yyyy-MM-dd} due {toInsert.DueDate:yyyy-MM-dd} in library [{libraryId}].");
            return this.Ok(toInsert);
        }

        [HttpPost("libraries/{libraryId}/copies/{copyId}/return")]
        public async Task<IActionResult> ReturnCopyAsync(long libraryId, long copyId, [FromBody] LibraryLoanReturn loanReturn, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Editor, cancellationToken);
            if (!await nsContext.LibraryCopies.AnyAsync(x => x.LibraryId == libraryId && x.Id == copyId, cancellationToken))
            {
                return this.NotFound($"Copy [{copyId}] not found in library [{libraryId}].");
            }
            LibraryLoan loanRecord = await nsContext.LibraryLoans.SingleOrDefaultAsync(x => x.CopyId == copyId && x.ReturnedDate == null, cancellationToken);
            if (loanRecord == default)
            {
                return this.Conflict($"Copy [{copyId}] is not on loan.");
            }
            DateOnly returnedDate = loanReturn.ReturnedDate ?? TodayUtc;
            if (returnedDate < loanRecord.LoanedDate)
            {
                return this.BadRequest($"The return date is before the loan date ({loanRecord.LoanedDate:yyyy-MM-dd}).");
            }

            loanRecord.ReturnedDate = returnedDate;
            loanRecord.ModifiedDate = DateTimeOffset.UtcNow;
            LibraryCopyEvent returned = this.NewEvent(LibraryCopyEventType.Returned, loanReturn.Notes);
            returned.CopyId = copyId;
            returned.LoanId = loanRecord.Id;
            _ = nsContext.LibraryCopyEvents.Add(returned);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] returned copy [{copyId}] from [{loanRecord.BorrowerName}] closing loan [{loanRecord.Id}] on {returnedDate:yyyy-MM-dd} in library [{libraryId}].");
            return this.Ok(loanRecord);
        }

        /// <summary>
        /// Adds a new active copy to the context, unsaved, with its <see cref="LibraryCopyEventType.Acquired"/> event, after
        /// checking its location and the owner's tier limit (a 409 when full). The caller sets the item, by id or navigation.
        /// Shared by creating a copy and adding one from a scan.
        /// </summary>
        private async Task<LibraryCopy> AddNewCopyAsync(NsContext nsContext, Library library, LibraryCopy copy, CancellationToken cancellationToken)
        {
            await EnsureLocationInLibraryAsync(nsContext, library.Id, copy.LocationId, cancellationToken);

            AccountTier ownerTier = await nsContext.Accounts.Where(x => x.Name == library.AccountName).Select(x => x.Tier).SingleAsync(cancellationToken);
            int existing = await nsContext.LibraryCopies.CountAsync(x => x.LibraryId == library.Id, cancellationToken);
            if (existing >= TierLimits.LibraryCopiesPerLibrary[ownerTier])
            {
                throw new ConflictException($"Library [{library.Id}] already has the most copies its owner's tier allows ({existing}).");
            }

            LibraryCopyEvent acquired = this.NewEvent(LibraryCopyEventType.Acquired);
            acquired.ToLocationId = copy.LocationId;
            LibraryCopy toInsert = new()
            {
                LibraryId = library.Id,
                LocationId = copy.LocationId,
                Status = LibraryCopyStatus.Active,
                Events = [acquired],
            };
            CopyEditableFields(copy, toInsert);
            _ = nsContext.LibraryCopies.Add(toInsert);
            return toInsert;
        }

        /// <summary>
        /// Everything a caller may set on a copy outside the dedicated actions.
        /// </summary>
        private static void CopyEditableFields(LibraryCopy from, LibraryCopy to)
        {
            to.Format = from.Format;
            to.Barcode = NormalizeBarcodeOrThrow(from.Barcode);
            // An ISBN barcode is the ISBN, so only a book printed with a UPC needs one entered separately.
            to.Isbn = NormalizeIsbnOrThrow(from.Isbn)
                ?? (to.Barcode != null && LibraryBarcode.TryParse(to.Barcode, out LibraryBarcode printed, out _) && printed.Kind == LibraryBarcodeKind.Isbn ? to.Barcode : null);
            to.Publisher = from.Publisher;
            to.PublishedYear = from.PublishedYear;
            to.Condition = from.Condition;
            to.AcquiredDate = from.AcquiredDate;
            to.AcquiredFrom = from.AcquiredFrom;
            to.PricePaid = from.PricePaid;
            to.Notes = from.Notes;
        }
    }
}
