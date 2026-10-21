using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Libraries
{
    [RequiresToken(AccountRoles.Library)]
    public sealed class EditModel : LibraryPageModel
    {
        public EditModel(NsTcpWtfClient nsClient) : base(nsClient) { }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Description { get; set; }

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

            this.Name = this.Library.Name;
            this.Description = this.Library.Description;
            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(long libraryId, CancellationToken cancellationToken)
        {
            if (await this.LoadLibraryAsync(libraryId, cancellationToken) is IActionResult notFound)
            {
                return notFound;
            }
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                this.ErrorMessage = "A library needs a name.";
                return this.Redirect($"/ui/library/{libraryId}/edit");
            }

            this.Library.Name = this.Name.Trim();
            this.Library.Description = this.Description?.Trim();
            this.Library.Shares = null;
            return await this.RunAndRedirectAsync(
                () => this.NsClient.Library.UpdateLibraryAsync(this.Library, cancellationToken),
                "Saved.",
                $"/ui/library/{libraryId}");
        }

        public async Task<IActionResult> OnPostDeleteAsync(long libraryId, CancellationToken cancellationToken)
        {
            try
            {
                await this.NsClient.Library.DeleteLibraryAsync(libraryId, cancellationToken);
                this.StatusMessage = "Library deleted.";
                return this.Redirect("/ui/library");
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ErrorText(ex);
                return this.Redirect($"/ui/library/{libraryId}/edit");
            }
        }
    }
}
