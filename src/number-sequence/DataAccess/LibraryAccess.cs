using Microsoft.EntityFrameworkCore;
using number_sequence.Exceptions;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.DataAccess
{
    /// <summary>
    /// The only place that decides which libraries an account can reach. Every library-scoped action goes through here
    /// rather than filtering on <c>AccountName == User.Identity.Name</c>. That Ledger pattern would work for the owner and
    /// silently lock out every account a library is shared with.
    /// <para>
    /// No access at all is a 404, so a library id doesn't reveal that the library exists. Access that is too low (a viewer
    /// trying to edit) is a 403, because that caller can already see the library and "not found" would be a lie.
    /// </para>
    /// </summary>
    public static class LibraryAccess
    {
        /// <summary>
        /// Loads a library (tracked, so the caller can modify it) with <see cref="Library.CallerPermission"/> set for
        /// <paramref name="accountName"/>.
        /// </summary>
        /// <exception cref="NotFoundException">The library doesn't exist, or the account has no access to it.</exception>
        /// <exception cref="ForbiddenException">The account can see the library but has less than <paramref name="minimum"/>.</exception>
        public static async Task<Library> GetLibraryWithAccessAsync(
            this NsContext nsContext,
            long libraryId,
            string accountName,
            LibraryPermission minimum,
            CancellationToken cancellationToken)
        {
            // Ownership is compared in sql rather than in c# so that it uses the same (case-insensitive) collation as the
            // account key itself.
            var row = await nsContext.Libraries
                .Where(x => x.Id == libraryId)
                .Select(x => new
                {
                    Library = x,
                    IsOwner = x.AccountName == accountName,
                    SharedPermission = x.Shares
                        .Where(s => s.AccountName == accountName)
                        .Select(s => (LibraryPermission?)s.Permission)
                        .FirstOrDefault(),
                })
                .SingleOrDefaultAsync(cancellationToken);

            LibraryPermission? permission = row == null ? null : row.IsOwner ? LibraryPermission.Owner : row.SharedPermission;
            if (permission == null)
            {
                throw new NotFoundException($"Library [{libraryId}] not found.");
            }
            if (permission < minimum)
            {
                throw new ForbiddenException($"Library [{libraryId}] requires {minimum} access; [{accountName}] has {permission}.");
            }

            row.Library.CallerPermission = permission.Value;
            return row.Library;
        }

        /// <summary>
        /// Every library <paramref name="accountName"/> owns or has been shared, untracked, each with
        /// <see cref="Library.CallerPermission"/> set. Ordered owned first, then by name.
        /// </summary>
        public static async Task<List<Library>> GetAccessibleLibrariesAsync(
            this NsContext nsContext,
            string accountName,
            CancellationToken cancellationToken)
        {
            var rows = await nsContext.Libraries
                .AsNoTracking()
                .Where(x => x.AccountName == accountName || x.Shares.Any(s => s.AccountName == accountName))
                .Select(x => new
                {
                    Library = x,
                    IsOwner = x.AccountName == accountName,
                    SharedPermission = x.Shares
                        .Where(s => s.AccountName == accountName)
                        .Select(s => (LibraryPermission?)s.Permission)
                        .FirstOrDefault(),
                })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                row.Library.CallerPermission = row.IsOwner ? LibraryPermission.Owner : row.SharedPermission.Value;
            }

            return rows
                .OrderByDescending(x => x.IsOwner)
                .ThenBy(x => x.Library.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(x => x.Library)
                .ToList();
        }

        /// <summary>
        /// The ids of every library <paramref name="accountName"/> can see, for queries that span libraries, such as the
        /// "do I own this?" barcode check.
        /// </summary>
        public static IQueryable<long> AccessibleLibraryIds(this NsContext nsContext, string accountName)
            => nsContext.Libraries
                .Where(x => x.AccountName == accountName || x.Shares.Any(s => s.AccountName == accountName))
                .Select(x => x.Id);
    }
}
