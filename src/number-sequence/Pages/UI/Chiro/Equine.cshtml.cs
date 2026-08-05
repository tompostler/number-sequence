using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Chiro
{
    [RequiresToken(AccountRoles.Chiro)]
    public sealed class EquineModel : ChiroFormModel
    {
        public EquineModel(NsTcpWtfClient nsClient, IOptions<Options.Email> emailOptions)
            : base(nsClient, emailOptions)
        {
        }

        #region Choices

        public static readonly string[] HeadOcciputChoices = ["left", "right", "bilateral superior", "bilateral inferior"];
        public static readonly string[] HeadTmjChoices = ["left", "right", "traction"];

        public static readonly string[] CervicalC1Choices = ["mobilization", "left", "right", "DALMA left", "DALMA right", "cranial left", "cranial right", "anterior ventral left", "anterior ventral right"];
        public static readonly string[] CervicalSpineRows = ["C2", "C3", "C4", "C5", "C6", "C7"];
        public static readonly string[] CervicalSpineColumns = ["mobilization", "left", "right", "anterior"];

        public static readonly string[] ThoracicSpineRows = ["T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13", "T14", "T15", "T16", "T17", "T18"];
        public static readonly string[] ThoracicSpineColumns = ["mobilization", "left", "right", "posterior"];
        public static readonly string[] RibsRows = ["R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10", "R11", "R12", "R13", "R14", "R15", "R16", "R17", "R18"];
        public static readonly string[] RibsColumns = ["left dorsal", "left cranial", "left caudal", "right dorsal", "right cranial", "right caudal"];
        public static readonly string[] SternumChoices = ["left", "right"];

        public static readonly string[] LumbarSpineRows = ["L1", "L2", "L3", "L4", "L5", "L6"];
        public static readonly string[] LumbarSpineColumns = ["mobilization", "left", "right", "posterior"];
        public static readonly string[] LumbarIntertransverseRows = ["L3/L4", "L4/L5", "L5/L6"];
        public static readonly string[] LumbarIntertransverseColumns = ["left", "right"];

        public static readonly string[] SacrumRows = ["Base", "Apex"];
        public static readonly string[] SacrumColumns = ["mobilization", "left", "right", "posterior"];
        public static readonly string[] PelvisRows = ["left", "right"];
        public static readonly string[] PelvisColumns = ["PI", "AS"];
        public static readonly string[] PelvisTractionChoices = ["traction"];

        public static readonly string[] ForelimbScapulaChoices = ["dorsal medial", "ventral lateral", "mobilization", "cranial", "caudal"];
        public static readonly string[] ForelimbHumorousChoices = ["external rotation technique 1", "external rotation technique 2", "internal rotation", "mobilization"];
        public static readonly string[] ForelimbUlnaChoices = ["caudal", "medial", "lateral", "mobilization"];
        public static readonly string[] ForelimbRadiusChoices = ["lateral", "mobilization"];
        public static readonly string[] ForelimbCarpusChoices = ["anterior hypomobility", "accessory hypomobility", "distal hypomobility", "mobilization"];
        public static readonly string[] ForelimbMetatarsalsPhalangesChoices = ["proximal Sesamoid hypomobility", "Pastern hypomobility", "mobilization"];

        public static readonly string[] RearLimbChoices = ["Coxo-Femoral cranial mobilization", "Coxo-Femoral caudal mobilization", "Dorsal-medial Patella", "Internally rotated Tibia", "Externally rotated Tibia", "Lateral Calcaneus", "Medial Calcaneus", "Dorsal Calcaneus", "Anterior Tarsal", "Phalange mobilization"];

        public static readonly string[] CoccygealChoices = ["traction", "thrust", "manipulation"];

        #endregion

        #region Bound form fields

        [BindProperty]
        public string HeadNotes { get; set; }
        [BindProperty]
        public string[] HeadOcciput { get; set; }
        [BindProperty]
        public string[] HeadTmj { get; set; }

        [BindProperty]
        public string CervicalNotes { get; set; }
        [BindProperty]
        public string[] CervicalC1 { get; set; }
        [BindProperty]
        public string[] CervicalSpine { get; set; }

        [BindProperty]
        public string ThoracicNotes { get; set; }
        [BindProperty]
        public string[] ThoracicSpine { get; set; }
        [BindProperty]
        public string[] Ribs { get; set; }
        [BindProperty]
        public string[] Sternum { get; set; }

        [BindProperty]
        public string LumbarNotes { get; set; }
        [BindProperty]
        public string[] LumbarSpine { get; set; }
        [BindProperty]
        public string[] LumbarIntertransverse { get; set; }

        [BindProperty]
        public string SacrumNotes { get; set; }
        [BindProperty]
        public string[] Sacrum { get; set; }
        [BindProperty]
        public string PelvicNotes { get; set; }
        [BindProperty]
        public string[] Pelvis { get; set; }
        [BindProperty]
        public string[] PelvisTraction { get; set; }

        [BindProperty]
        public string LeftForelimbNotes { get; set; }
        [BindProperty]
        public string[] LeftForelimbScapula { get; set; }
        [BindProperty]
        public string[] LeftForelimbHumorous { get; set; }
        [BindProperty]
        public string[] LeftForelimbUlna { get; set; }
        [BindProperty]
        public string[] LeftForelimbRadius { get; set; }
        [BindProperty]
        public string[] LeftForelimbCarpus { get; set; }
        [BindProperty]
        public string[] LeftForelimbMetatarsalsPhalanges { get; set; }

        [BindProperty]
        public string RightForelimbNotes { get; set; }
        [BindProperty]
        public string[] RightForelimbScapula { get; set; }
        [BindProperty]
        public string[] RightForelimbHumorous { get; set; }
        [BindProperty]
        public string[] RightForelimbUlna { get; set; }
        [BindProperty]
        public string[] RightForelimbRadius { get; set; }
        [BindProperty]
        public string[] RightForelimbCarpus { get; set; }
        [BindProperty]
        public string[] RightForelimbMetatarsalsPhalanges { get; set; }

        [BindProperty]
        public string LeftRearLimbNotes { get; set; }
        [BindProperty]
        public string[] LeftRearLimb { get; set; }

        [BindProperty]
        public string RightRearLimbNotes { get; set; }
        [BindProperty]
        public string[] RightRearLimb { get; set; }

        [BindProperty]
        public string CoccygealNotes { get; set; }
        [BindProperty]
        public string[] Coccygeal { get; set; }

        #endregion

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            string[] cervicalSpine = ChiroForm.JoinGrid(this.CervicalSpine, CervicalSpineRows, CervicalSpineColumns);
            string[] sacrum = ChiroForm.JoinGrid(this.Sacrum, SacrumRows, SacrumColumns);
            string[] pelvis = ChiroForm.JoinGrid(this.Pelvis, PelvisRows, PelvisColumns);

            ChiroInput input = new()
            {
                PatientName = this.PatientName,
                OwnerName = this.OwnerName,
                DateOfService = new DateTimeOffset(this.DateOfService.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
                CcEmails = ChiroForm.SplitEmails(this.AdditionalRecipient),
                ClinicAbbreviation = this.ClinicAbbreviation,

                HeadNotes = this.HeadNotes,
                HeadOcciput = ChiroForm.Join(this.HeadOcciput),
                HeadTmj = ChiroForm.Join(this.HeadTmj),

                CervicalNotes = this.CervicalNotes,
                Cervical = [ChiroForm.Join(this.CervicalC1), .. cervicalSpine],

                ThoracicNotes = this.ThoracicNotes,
                Sternum = ChiroForm.Join(this.Sternum),
                Thoracic = ChiroForm.JoinGrid(this.ThoracicSpine, ThoracicSpineRows, ThoracicSpineColumns),

                Ribs = ChiroForm.JoinGrid(this.Ribs, RibsRows, RibsColumns),

                LumbarNotes = this.LumbarNotes,
                Lumbar = ChiroForm.JoinGrid(this.LumbarSpine, LumbarSpineRows, LumbarSpineColumns),
                LumbarIntertransverse = ChiroForm.JoinGrid(this.LumbarIntertransverse, LumbarIntertransverseRows, LumbarIntertransverseColumns),

                SacrumNotes = this.SacrumNotes,
                SacrumBase = sacrum[0],
                SacrumApex = sacrum[1],

                PelvicNotes = this.PelvicNotes,
                PelvicLeft = pelvis[0],
                PelvicRight = pelvis[1],
                PelvicTraction = ChiroForm.Join(this.PelvisTraction),

                LeftForelimbNotes = this.LeftForelimbNotes,
                LeftForelimbScapula = ChiroForm.Join(this.LeftForelimbScapula),
                LeftForelimbHumerus = ChiroForm.Join(this.LeftForelimbHumorous),
                LeftForelimbUlna = ChiroForm.Join(this.LeftForelimbUlna),
                LeftForelimbRadius = ChiroForm.Join(this.LeftForelimbRadius),
                LeftForelimbCarpus = ChiroForm.Join(this.LeftForelimbCarpus),
                LeftForelimbMetatarsalsPhalanges = ChiroForm.Join(this.LeftForelimbMetatarsalsPhalanges),

                RightForelimbNotes = this.RightForelimbNotes,
                RightForelimbScapula = ChiroForm.Join(this.RightForelimbScapula),
                RightForelimbHumerus = ChiroForm.Join(this.RightForelimbHumorous),
                RightForelimbUlna = ChiroForm.Join(this.RightForelimbUlna),
                RightForelimbRadius = ChiroForm.Join(this.RightForelimbRadius),
                RightForelimbCarpus = ChiroForm.Join(this.RightForelimbCarpus),
                RightForelimbMetatarsalsPhalanges = ChiroForm.Join(this.RightForelimbMetatarsalsPhalanges),

                LeftRearLimbNotes = this.LeftRearLimbNotes,
                LeftRearLimb = ChiroForm.Join(this.LeftRearLimb),

                RightRearLimbNotes = this.RightRearLimbNotes,
                RightRearLimb = ChiroForm.Join(this.RightRearLimb),

                CoccygealNotes = this.CoccygealNotes,
                Coccygeal = ChiroForm.Join(this.Coccygeal),

                Other = this.ExtendedOtherNotes,
            };

            try
            {
                ChiroInputCreated created = await this.NsClient.Chiro.SubmitAsync(ChiroSpecies.Equine, input, cancellationToken);
                return this.Redirect($"/ui/chiro?submitted=equine&rowId={created.RowId}");
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }
        }
    }
}
