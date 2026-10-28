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
        /// Loans in a library, each with its copy and the copy's item, so a loans page needs no further calls. Open
        /// loans sort by due date (those with none last), then oldest first; with <paramref name="open"/> false, every loan
        /// sorts newest first.
        /// </summary>
        [HttpGet("libraries/{libraryId}/loans")]
        public async Task<IActionResult> GetLoansAsync(
            long libraryId,
            CancellationToken cancellationToken,
            [FromQuery] bool open = true,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100)
        {
            if (take < 1 || take > 500)
            {
                return this.BadRequest("Take must be between 1 and 500.");
            }

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Viewer, cancellationToken);

            IQueryable<LibraryLoan> query = nsContext.LibraryLoans
                .AsNoTracking()
                .Include(x => x.Copy)
                    .ThenInclude(x => x.Item)
                .Where(x => x.Copy.LibraryId == libraryId);
            query = open
                ? query.Where(x => x.ReturnedDate == null).OrderBy(x => x.DueDate == null).ThenBy(x => x.DueDate).ThenBy(x => x.LoanedDate).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.LoanedDate).ThenByDescending(x => x.Id);

            List<LibraryLoan> loans = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return this.Ok(loans);
        }

        /// <summary>
        /// "Do I already own this?" across every library the caller can see, so the shopping case doesn't start by picking a
        /// library. Returns matching copies that aren't disposed, each with its item and open loan; the caller maps
        /// <see cref="LibraryCopy.LibraryId"/> to a name from the library list it already has. This only matches the exact
        /// barcode, so a different edition of something owned won't show up here.
        /// </summary>
        [HttpGet("barcodes/{barcode}")]
        public async Task<IActionResult> GetCopiesByBarcodeAsync(string barcode, CancellationToken cancellationToken)
        {
            string normalized = NormalizeBarcodeOrThrow(barcode) ?? throw new BadRequestException("Barcode is empty.");

            Stopwatch sw = Stopwatch.StartNew();
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            IQueryable<long> libraryIds = nsContext.AccessibleLibraryIds(this.AccountName);
            List<LibraryCopy> copies = await nsContext.LibraryCopies
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Item)
                .Include(x => x.Loans.Where(l => l.ReturnedDate == null))
                .Where(LibraryBarcode.MatchesCopy(normalized))
                .Where(x => libraryIds.Contains(x.LibraryId) && x.Status != LibraryCopyStatus.Disposed)
                .OrderBy(x => x.LibraryId)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] looked up barcode [{barcode}] as [{normalized}]: {copies.Count} copies in {sw.ElapsedMilliseconds}ms.");
            return this.Ok(copies);
        }
    }
}
