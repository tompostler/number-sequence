using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Exceptions;
using number_sequence.Filters;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Controllers
{
    /// <summary>
    /// Every action that touches one library gets it from <see cref="LibraryAccess.GetLibraryWithAccessAsync"/> first, and
    /// scopes every other query by that library's id. Nothing here filters on the caller's account name directly; see
    /// <see cref="LibraryAccess"/> for why.
    /// </summary>
    [ApiController, Route("[controller]"), RequiresToken(AccountRoles.Library)]
    public sealed partial class LibraryController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<LibraryController> logger;

        public LibraryController(
            IServiceProvider serviceProvider,
            ILogger<LibraryController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        private string AccountName => this.User.Identity.Name;

        private static DateOnly TodayUtc => DateOnly.FromDateTime(DateTime.UtcNow);

        /// <summary>
        /// Normalizes a barcode through <see cref="LibraryBarcode.TryParse"/>. Blank stays null (a copy doesn't need one);
        /// anything else that doesn't parse is a 400 with the reason.
        /// </summary>
        private static string NormalizeBarcodeOrThrow(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            if (!LibraryBarcode.TryParse(input, out LibraryBarcode barcode, out string error))
            {
                throw new BadRequestException(error);
            }
            return barcode.Value;
        }

        /// <summary>
        /// A 400 unless <paramref name="locationId"/> is null or a location in <paramref name="libraryId"/>.
        /// </summary>
        private static async Task EnsureLocationInLibraryAsync(NsContext nsContext, long libraryId, long? locationId, CancellationToken cancellationToken)
        {
            if (locationId.HasValue && !await nsContext.LibraryLocations.AnyAsync(x => x.Id == locationId && x.LibraryId == libraryId, cancellationToken))
            {
                throw new BadRequestException($"Location [{locationId}] is not in library [{libraryId}].");
            }
        }

        /// <summary>
        /// A 400 unless <paramref name="accountName"/> is null or an existing account. Returns the name as stored, which is
        /// lowercase.
        /// </summary>
        private static async Task<string> EnsureAccountExistsAsync(NsContext nsContext, string accountName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                return null;
            }
            string lowered = accountName.Trim().ToLower();
            if (!await nsContext.Accounts.AnyAsync(x => x.Name == lowered, cancellationToken))
            {
                throw new BadRequestException($"Account [{accountName}] does not exist.");
            }
            return lowered;
        }

        private LibraryCopyEvent NewEvent(LibraryCopyEventType eventType, string notes = null)
            => new()
            {
                EventType = eventType,
                AccountName = this.AccountName,
                Notes = notes,
            };

        /// <summary>
        /// Runs <paramref name="work"/> in a transaction under the context's execution strategy, which is required once
        /// retry-on-failure is enabled. Only for <c>ExecuteDelete</c>/<c>ExecuteUpdate</c> work: those bypass the change
        /// tracker, so a retry re-running the delegate starts from the same state the first attempt did.
        /// </summary>
        private static Task InBulkTransactionAsync(NsContext nsContext, Func<CancellationToken, Task> work, CancellationToken cancellationToken)
            => nsContext.Database.CreateExecutionStrategy().ExecuteAsync(
                async ct =>
                {
                    await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await nsContext.Database.BeginTransactionAsync(ct);
                    await work(ct);
                    await transaction.CommitAsync(ct);
                },
                cancellationToken);
    }
}
