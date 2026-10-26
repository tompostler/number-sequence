using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Controllers
{
    public sealed partial class LibraryController
    {
        [HttpGet("libraries")]
        public async Task<IActionResult> GetLibrariesAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<Library> libraries = await nsContext.GetAccessibleLibrariesAsync(this.AccountName, cancellationToken);
            return this.Ok(libraries);
        }

        [HttpPost("libraries")]
        public async Task<IActionResult> CreateLibraryAsync([FromBody] Library library, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Account account = await nsContext.Accounts.SingleAsync(x => x.Name == this.AccountName, cancellationToken);
            int owned = await nsContext.Libraries.CountAsync(x => x.AccountName == account.Name, cancellationToken);
            if (owned >= TierLimits.LibrariesPerAccount[account.Tier])
            {
                return this.Conflict($"Too many libraries already created for account with name [{account.Name}].");
            }

            Library toInsert = new()
            {
                AccountName = account.Name,
                Name = library.Name,
                Description = library.Description,
            };
            _ = nsContext.Libraries.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            toInsert.CallerPermission = LibraryPermission.Owner;
            this.logger.LogInformation($"[{this.AccountName}] created library [{toInsert.Id}] [{toInsert.Name}].");
            return this.Ok(toInsert);
        }

        [HttpGet("libraries/{libraryId}")]
        public async Task<IActionResult> GetLibraryAsync(long libraryId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Library library = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Viewer, cancellationToken);
            return this.Ok(library);
        }

        [HttpPut("libraries")]
        public async Task<IActionResult> UpdateLibraryAsync([FromBody] Library library, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Library libraryRecord = await nsContext.GetLibraryWithAccessAsync(library.Id, this.AccountName, LibraryPermission.Owner, cancellationToken);
            libraryRecord.Name = library.Name;
            libraryRecord.Description = library.Description;
            libraryRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] updated library [{libraryRecord.Id}] [{libraryRecord.Name}].");
            return this.Ok(libraryRecord);
        }

        /// <summary>
        /// Deletes in an explicit order instead of relying on one cascading delete of the library row. Copies are reachable
        /// from the library both directly (no action) and through items (cascade), and a single statement would depend on
        /// sql server checking the no-action keys only after its cascades have run. Copies go first (taking loans and events
        /// with them), then scan sessions (which hold no-action keys to locations), then the library, which cascades to
        /// items, locations and shares.
        /// </summary>
        [HttpDelete("libraries/{libraryId}")]
        public async Task<IActionResult> DeleteLibraryAsync(long libraryId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Library library = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Owner, cancellationToken);

            List<long> coveredItemIds = await nsContext.LibraryItems.Where(x => x.LibraryId == libraryId && x.HasCover).Select(x => x.Id).ToListAsync(cancellationToken);
            int copies = 0, sessions = 0;
            await InBulkTransactionAsync(
                nsContext,
                async ct =>
                {
                    copies = await nsContext.LibraryCopies.Where(x => x.LibraryId == libraryId).ExecuteDeleteAsync(ct);
                    sessions = await nsContext.LibraryScanSessions.Where(x => x.LibraryId == libraryId).ExecuteDeleteAsync(ct);
                    _ = await nsContext.Libraries.Where(x => x.Id == libraryId).ExecuteDeleteAsync(ct);
                },
                cancellationToken);

            await this.DeleteCoverBlobsAsync(coveredItemIds, cancellationToken);
            this.logger.LogInformation($"[{this.AccountName}] deleted library [{libraryId}] [{library.Name}] with {copies} copies and {sessions} scan sessions.");
            return this.NoContent();
        }

        [HttpGet("libraries/{libraryId}/shares")]
        public async Task<IActionResult> GetSharesAsync(long libraryId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Owner, cancellationToken);
            List<LibraryShare> shares = await nsContext.LibraryShares
                .AsNoTracking()
                .Where(x => x.LibraryId == libraryId)
                .OrderBy(x => x.AccountName)
                .ToListAsync(cancellationToken);
            return this.Ok(shares);
        }

        /// <summary>
        /// Adds a share, or changes the permission of an existing one.
        /// </summary>
        [HttpPut("libraries/{libraryId}/shares")]
        public async Task<IActionResult> PutShareAsync(long libraryId, [FromBody] LibraryShare share, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Library library = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, LibraryPermission.Owner, cancellationToken);
            if (share.Permission == LibraryPermission.Owner)
            {
                return this.BadRequest($"A share can be {LibraryPermission.Viewer} or {LibraryPermission.Editor}; the owner is the library's account.");
            }
            string accountName = await EnsureAccountExistsAsync(nsContext, share.AccountName, cancellationToken);
            if (accountName == library.AccountName)
            {
                return this.BadRequest("The owner can't be shared their own library.");
            }

            LibraryShare shareRecord = await nsContext.LibraryShares.SingleOrDefaultAsync(x => x.LibraryId == libraryId && x.AccountName == accountName, cancellationToken);
            if (shareRecord == default)
            {
                shareRecord = new() { LibraryId = libraryId, AccountName = accountName, Permission = share.Permission };
                _ = nsContext.LibraryShares.Add(shareRecord);
            }
            else
            {
                shareRecord.Permission = share.Permission;
                shareRecord.ModifiedDate = DateTimeOffset.UtcNow;
            }
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation($"[{this.AccountName}] shared library [{libraryId}] with [{accountName}] as {share.Permission}.");
            return this.Ok(shareRecord);
        }

        /// <summary>
        /// The owner can remove anyone's share. Anyone can remove their own, which is how a library is left.
        /// </summary>
        [HttpDelete("libraries/{libraryId}/shares/{accountName}")]
        public async Task<IActionResult> DeleteShareAsync(long libraryId, string accountName, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            string lowered = accountName.Trim().ToLower();
            LibraryPermission minimum = lowered == this.AccountName ? LibraryPermission.Viewer : LibraryPermission.Owner;
            _ = await nsContext.GetLibraryWithAccessAsync(libraryId, this.AccountName, minimum, cancellationToken);

            int deleted = await nsContext.LibraryShares.Where(x => x.LibraryId == libraryId && x.AccountName == lowered).ExecuteDeleteAsync(cancellationToken);
            if (deleted == 0)
            {
                return this.NotFound($"Library [{libraryId}] is not shared with [{accountName}].");
            }

            this.logger.LogInformation($"[{this.AccountName}] removed [{lowered}]'s share of library [{libraryId}].");
            return this.NoContent();
        }
    }
}
