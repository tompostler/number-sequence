using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Pages.UI.Libraries
{
    [RequiresToken(AccountRoles.Library)]
    public sealed class SharesModel : LibraryPageModel
    {
        public SharesModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        public List<LibraryShare> Shares { get; private set; }

        public async Task<IActionResult> OnGetAsync(long libraryId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }
            if (!this.IsOwner)
            {
                return this.Redirect($"/ui/library/{libraryId}");
            }

            this.Shares = await this.NsClient.Library.GetSharesAsync(libraryId, cancellationToken);
            return this.Page();
        }

        public async Task<IActionResult> OnPostSetAsync(long libraryId, string accountName, LibraryPermission permission, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                this.ErrorMessage = "Enter the account name to share with.";
                return this.Redirect($"/ui/library/{libraryId}/shares");
            }

            return await this.RunAndRedirectAsync(
                () => this.NsClient.Library.PutShareAsync(libraryId, new LibraryShare { AccountName = accountName.Trim(), Permission = permission }, cancellationToken),
                $"{accountName.Trim()} is now a {permission.ToString().ToLowerInvariant()}. They also need the {AccountRoles.Library} role to open it.",
                $"/ui/library/{libraryId}/shares");
        }

        public async Task<IActionResult> OnPostRemoveAsync(long libraryId, string accountName, CancellationToken cancellationToken)
            => await this.RunAndRedirectAsync(
                () => this.NsClient.Library.DeleteShareAsync(libraryId, accountName, cancellationToken),
                $"Stopped sharing with {accountName}.",
                $"/ui/library/{libraryId}/shares");
    }
}
