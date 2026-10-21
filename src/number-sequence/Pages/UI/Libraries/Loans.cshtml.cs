using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries
{
    [RequiresToken(AccountRoles.Library)]
    public sealed class LoansModel : LibraryPageModel
    {
        private const int Take = 200;

        public LoansModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        [BindProperty(SupportsGet = true)]
        public bool All { get; set; }

        public List<LibraryLoan> Loans { get; private set; }

        public bool Truncated { get; private set; }

        public async Task<IActionResult> OnGetAsync(long libraryId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }

            // The server's order: open loans soonest due first, so anything overdue is at the top; all loans newest first.
            this.Loans = await this.NsClient.Library.GetLoansAsync(libraryId, open: !this.All, take: Take, cancellationToken: cancellationToken);
            this.Truncated = this.Loans.Count == Take;
            return this.Page();
        }

        public async Task<IActionResult> OnPostReturnAsync(long libraryId, long copyId, string title, CancellationToken cancellationToken)
            => await this.RunAndRedirectAsync(
                () => this.NsClient.Library.ReturnCopyAsync(libraryId, copyId, new LibraryLoanReturn(), cancellationToken),
                $"Returned {title}.",
                $"/ui/library/{libraryId}/loans");
    }
}
