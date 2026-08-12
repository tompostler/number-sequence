using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using number_sequence.Services;
using number_sequence.Utilities;
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
            this.Clinics = emailOptions.Value.ChiroBatchMapParsed;
        }

        /// <summary>
        /// The clinics a record can additionally be sent to, taken from the batch email map so the dropdown always
        /// matches the clinics the batch sender knows how to route. Keyed by abbreviation.
        /// </summary>
        public IReadOnlyDictionary<string, Options.ChiroClinic> Clinics { get; }

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

        /// <summary>
        /// The pasted dictation. Only ever an input to parsing; it is not part of the record and is not submitted.
        /// </summary>
        [BindProperty]
        public string Transcript { get; set; }

        /// <summary>
        /// What the parser was unsure about, shown above the form after a parse. Discarded on submit.
        /// </summary>
        public IReadOnlyList<ChiroParseFlag> ParseFlags { get; private set; } = [];

        /// <summary>
        /// What the last parse took and cost, shown under the transcript. Null until a parse has run, and gone
        /// again on the next page load; it describes the request, not the record.
        /// </summary>
        public ChiroParseUsage ParseUsage { get; private set; }

        public long ParseElapsedMilliseconds { get; private set; }

        public string ErrorMessage { get; protected set; }

        public virtual IActionResult OnGet()
        {
            this.DateOfService = DateOnly.FromDateTime(DateTime.Now);
            return this.Page();
        }

        /// <summary>
        /// Overwrites the form with a parsed draft. Values the parser had nothing to say about are left alone, so a
        /// second parse cannot silently blank out something the user already typed.
        /// </summary>
        protected void ApplyParse(ChiroVocabulary vocabulary, ChiroParseResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.PatientName))
            {
                this.PatientName = result.PatientName;
            }

            if (!string.IsNullOrWhiteSpace(result.OwnerName))
            {
                this.OwnerName = result.OwnerName;
            }

            if (result.DateOfService.HasValue)
            {
                this.DateOfService = result.DateOfService.Value;
            }

            if (!string.IsNullOrWhiteSpace(result.ClinicAbbreviation) && this.Clinics.ContainsKey(result.ClinicAbbreviation))
            {
                this.ClinicAbbreviation = result.ClinicAbbreviation;
            }

            ChiroForm.ApplyParse(this, vocabulary, result);
            this.ParseFlags = result.Flags;
            this.ParseUsage = result.Usage;
            this.ParseElapsedMilliseconds = result.ElapsedMilliseconds;

            // Tag helpers render from ModelState in preference to the model, and ModelState still holds whatever was
            // posted. Without this the page would come back showing the pre-parse values.
            this.ModelState.Clear();
        }
    }
}
