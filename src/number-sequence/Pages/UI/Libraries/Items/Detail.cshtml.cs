using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries.Items
{
    /// <summary>
    /// One item with its copies and their history. Every copy action is a small form posting here, then redirecting back.
    /// </summary>
    [RequiresToken(AccountRoles.Library)]
    public sealed class DetailModel : LibraryPageModel
    {
        public DetailModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        public LibraryItem Item { get; private set; }

        public List<LibraryLocation> Locations { get; private set; }

        public Dictionary<long, string> Paths { get; private set; }

        public Dictionary<long, LibraryLoan> LoansById { get; private set; }

        /// <summary>
        /// Every copy's events together, newest first.
        /// </summary>
        public List<LibraryCopyEvent> History { get; private set; }

        public async Task<IActionResult> OnGetAsync(long libraryId, long itemId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }

            try
            {
                this.Item = await this.NsClient.Library.GetItemAsync(libraryId, itemId, cancellationToken);
            }
            catch (NsTcpWtfClientException)
            {
                return this.NotFound();
            }

            this.Locations = await this.NsClient.Library.GetLocationsAsync(libraryId, cancellationToken);
            this.Paths = LibraryDisplay.Paths(this.Locations);
            this.LoansById = this.Item.Copies.SelectMany(x => x.Loans).ToDictionary(x => x.Id);
            this.History = this.Item.Copies
                .SelectMany(x => x.Events)
                .OrderByDescending(x => x.OccurredDate)
                .ThenByDescending(x => x.Id)
                .ToList();
            return this.Page();
        }

        private string Here(long libraryId, long itemId) => $"/ui/library/{libraryId}/items/{itemId}";

        public async Task<IActionResult> OnPostMoveAsync(long libraryId, long itemId, long copyId, long? locationId, string notes, CancellationToken cancellationToken)
            => await this.RunAndRedirectAsync(
                () => this.NsClient.Library.MoveCopyAsync(libraryId, copyId, new LibraryCopyMove { LocationId = locationId, Notes = notes }, cancellationToken),
                "Moved.",
                this.Here(libraryId, itemId));

        public async Task<IActionResult> OnPostLendAsync(
            long libraryId,
            long itemId,
            long copyId,
            string borrowerName,
            string borrowerAccountName,
            DateOnly? loanedDate,
            DateOnly? dueDate,
            string notes,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(borrowerName) && string.IsNullOrWhiteSpace(borrowerAccountName))
            {
                this.ErrorMessage = "Who is borrowing it?";
                return this.Redirect(this.Here(libraryId, itemId));
            }

            LibraryLoan loan = new()
            {
                // An account alone is enough; its name stands in for the borrower's.
                BorrowerName = string.IsNullOrWhiteSpace(borrowerName) ? borrowerAccountName.Trim() : borrowerName.Trim(),
                BorrowerAccountName = string.IsNullOrWhiteSpace(borrowerAccountName) ? null : borrowerAccountName.Trim(),
                LoanedDate = loanedDate ?? default,
                DueDate = dueDate,
                Notes = notes,
            };
            return await this.RunAndRedirectAsync(
                () => this.NsClient.Library.LoanCopyAsync(libraryId, copyId, loan, cancellationToken),
                $"Lent to {loan.BorrowerName}.",
                this.Here(libraryId, itemId));
        }

        public async Task<IActionResult> OnPostReturnAsync(long libraryId, long itemId, long copyId, DateOnly? returnedDate, string notes, CancellationToken cancellationToken)
            => await this.RunAndRedirectAsync(
                () => this.NsClient.Library.ReturnCopyAsync(libraryId, copyId, new LibraryLoanReturn { ReturnedDate = returnedDate, Notes = notes }, cancellationToken),
                "Returned.",
                this.Here(libraryId, itemId));

        public async Task<IActionResult> OnPostStatusAsync(long libraryId, long itemId, long copyId, LibraryCopyStatus status, string notes, CancellationToken cancellationToken)
            => await this.RunAndRedirectAsync(
                () => this.NsClient.Library.SetCopyStatusAsync(libraryId, copyId, new LibraryCopyStatusChange { Status = status, Notes = notes }, cancellationToken),
                status switch
                {
                    LibraryCopyStatus.Missing => "Marked missing.",
                    LibraryCopyStatus.Disposed => "Disposed. Its history is kept.",
                    _ => "Restored.",
                },
                this.Here(libraryId, itemId));

        public async Task<IActionResult> OnPostDeleteItemAsync(long libraryId, long itemId, CancellationToken cancellationToken)
        {
            try
            {
                await this.NsClient.Library.DeleteItemAsync(libraryId, itemId, cancellationToken);
                this.StatusMessage = "Item deleted.";
                return this.Redirect($"/ui/library/{libraryId}");
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ErrorText(ex);
                return this.Redirect(this.Here(libraryId, itemId));
            }
        }
    }
}
