using Microsoft.AspNetCore.Mvc.RazorPages;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Chiro
{
    [RequiresToken(AccountRoles.Chiro)]
    public sealed class IndexModel : PageModel
    {
        public string SubmittedSpecies { get; private set; }
        public string SubmittedRowId { get; private set; }

        /// <summary>
        /// What the pdf will be called once the orchestration has run, so the doctor can recognise it in the email
        /// that follows. Named before it exists; see <see cref="ChiroFormModel.RedirectToSubmitted"/>.
        /// </summary>
        public string SubmittedDocument { get; private set; }

        public void OnGet()
        {
            this.SubmittedSpecies = this.TempData[ChiroForm.SubmittedSpeciesKey] as string;
            this.SubmittedRowId = this.TempData[ChiroForm.SubmittedRowIdKey] as string;
            this.SubmittedDocument = this.TempData[ChiroForm.SubmittedDocumentKey] as string;
        }
    }
}
