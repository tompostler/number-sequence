using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using TcpWtf.NumberSequence.Client;

namespace number_sequence.Pages.UI.Chiro
{
    /// <summary>
    /// The intake fields and plumbing common to every species form. Species-specific fields, choices, and the
    /// mapping into <c>ChiroInput</c> stay on the derived model; only the shared patient/owner/recipient intake
    /// lives here so the <c>_ChiroFormIntake</c> partial can bind against it.
    /// </summary>
    public abstract class ChiroFormModel : PageModel
    {
        protected readonly NsTcpWtfClient NsClient;

        protected ChiroFormModel(NsTcpWtfClient nsClient, IOptions<Options.Email> emailOptions)
        {
            this.NsClient = nsClient;
            this.ClinicChoices = [.. emailOptions.Value.ChiroBatchMapParsed.Keys];
        }

        /// <summary>
        /// The clinics a record can additionally be sent to, taken from the keys of the batch email map so the
        /// dropdown always matches the clinics the batch sender knows how to route.
        /// </summary>
        public string[] ClinicChoices { get; }

        [BindProperty, Required, MaxLength(128)]
        public string PatientName { get; set; }

        [BindProperty, Required, MaxLength(128)]
        public string OwnerName { get; set; }

        [BindProperty, Required]
        public DateOnly DateOfService { get; set; }

        [BindProperty]
        public string ClinicAbbreviation { get; set; }

        [BindProperty]
        public string AdditionalRecipient { get; set; }

        [BindProperty]
        public string ExtendedOtherNotes { get; set; }

        public string ErrorMessage { get; protected set; }

        public virtual IActionResult OnGet()
        {
            this.DateOfService = DateOnly.FromDateTime(DateTime.Now);
            return this.Page();
        }
    }
}
