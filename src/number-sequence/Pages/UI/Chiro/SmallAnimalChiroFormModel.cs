using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using number_sequence.Services;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Pages.UI.Chiro
{
    /// <summary>
    /// The whole canine and feline form: the choices, the bound fields, the parse handler, and the mapping into a
    /// <see cref="ChiroInput"/>. The two species differ by name and diagram and by nothing else, so they share this
    /// rather than keeping two copies that have to be edited in step; a page supplies only its
    /// <see cref="Vocabulary"/>.
    /// <para>
    /// This is not a general base for every species. Equine has its own model because its form genuinely differs,
    /// and a future species that differs should get its own too. Bending this one with per-species conditionals
    /// would cost more than the copy it saves.
    /// </para>
    /// </summary>
    public abstract class SmallAnimalChiroFormModel : ChiroFormModel
    {
        private readonly ChiroDictationParser parser;

        protected SmallAnimalChiroFormModel(
            NsTcpWtfClient nsClient,
            IOptions<Options.Email> emailOptions,
            ChiroDictationParser parser)
            : base(nsClient, emailOptions)
        {
            this.parser = parser;
        }

        /// <summary>
        /// The only thing a page has to supply. Built once per page type from <see cref="BuildVocabulary"/>, since
        /// the species is all that varies.
        /// </summary>
        public abstract ChiroVocabulary Vocabulary { get; }

        #region Choices

        public static readonly string[] HeadOcciputChoices = ["cranial left", "cranial right", "bilateral superior", "bilateral inferior"];
        public static readonly string[] HeadTmjChoices = ["ventral mandible technique 1 left", "ventral mandible technique 1 right", "ventral mandible technique 2 left", "ventral mandible technique 2 right", "rostral mandible", "traction", "upper left", "upper right"];

        public static readonly string[] CervicalC1Choices = ["mobilization", "dorsal left", "dorsal right", "cranial technique 1 left", "cranial technique 1 right"];
        public static readonly string[] CervicalSpineRows = ["C2", "C3", "C4", "C5", "C6", "C7"];
        public static readonly string[] CervicalSpineColumns = ["mobilization", "left", "right", "posterior"];

        public static readonly string[] ThoracicSpineRows = ["T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", "T13"];
        public static readonly string[] ThoracicSpineColumns = ["mobilization", "left", "right", "posterior"];
        public static readonly string[] RibsRows = ["R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10", "R11", "R12", "R13"];
        public static readonly string[] RibsColumns = ["left dorsal", "left cranial", "left caudal", "right dorsal", "right cranial", "right caudal"];
        public static readonly string[] SternumChoices = ["left", "right"];

        public static readonly string[] LumbarSpineRows = ["L1", "L2", "L3", "L4", "L5", "L6", "L7"];
        public static readonly string[] LumbarSpineColumns = ["mobilization", "left", "right", "posterior dorsal process (SCP)", "posterior mammillary process (SCP)"];

        public static readonly string[] SacrumRows = ["Base", "Apex"];
        public static readonly string[] SacrumColumns = ["mobilization", "left", "right", "posterior"];
        public static readonly string[] PelvisRows = ["left", "right"];
        public static readonly string[] PelvisColumns = ["PI", "AS"];
        public static readonly string[] PelvisTractionChoices = ["traction"];

        public static readonly string[] ForelimbScapulaChoices = ["dorsal medial", "ventral lateral", "mobilization"];
        public static readonly string[] ForelimbHumorousChoices = ["external rotation technique 1", "external rotation technique 2", "internal rotation", "mobilization"];
        public static readonly string[] ForelimbUlnaChoices = ["caudal", "medial", "lateral", "mobilization"];
        public static readonly string[] ForelimbRadiusChoices = ["lateral", "mobilization"];
        public static readonly string[] ForelimbCarpusChoices = ["anterior hypomobility", "accessory hypomobility", "mobilization"];
        public static readonly string[] ForelimbMetatarsalsPhalangesChoices = ["digit 1 hypomobility", "digit 2 hypomobility", "digit 3 hypomobility", "digit 4 hypomobility", "digit 5 hypomobility", "mobilization"];

        public static readonly string[] RearLimbChoices = ["Coxo-Femoral cranial mobilization", "Coxo-Femoral caudal mobilization", "Internally rotated Tibia", "Externally rotated Tibia", "Lateral Calcaneus", "Medial Calcaneus", "Dorsal Calcaneus", "Anterior Tarsal", "Tarsal traction", "digit 1 hypomobility", "digit 2 hypomobility", "digit 3 hypomobility", "digit 4 hypomobility", "Phalange mobilization"];

        public static readonly string[] CoccygealChoices = ["traction", "thrust", "manipulation"];

        /// <summary>
        /// Joints the doctor assesses at every visit, so their mobilizations are recorded unless the dictation named
        /// one of that joint's own options. Phalange mobilization is a standard evaluation and stands regardless of
        /// any digit findings, which are a separate observation rather than a contradiction of it.
        /// </summary>
        public static readonly string[] RearLimbStandingSites = ["Coxo-Femoral", "Phalange"];

        #endregion

        /// <summary>
        /// The same arrays the page renders from, described for the dictation parser. Every name is a
        /// <c>nameof</c> of the property it fills, so the parser's schema and the checkboxes cannot disagree.
        /// </summary>
        protected static ChiroVocabulary BuildVocabulary(ChiroSpecies species)
        {
            // A dog gets a full workup every visit, so a limb the dictation never mentions was still assessed and
            // found to need nothing. A cat gets worked on where it lets you, so the same silence means the limb was
            // not done at all. The spine keeps the default for both; this is only about the limbs.
            bool limbsAreAssessedEveryVisit = species != ChiroSpecies.Feline;

            ChiroVocabularyGroup Limb(
                string name,
                string label,
                string[] choices,
                string notesName,
                string[] standingSites = null,
                bool expandsWhenUnqualified = false)
                => new(
                    name,
                    label,
                    choices,
                    notesName,
                    limbsAreAssessedEveryVisit ? standingSites : [],
                    expandsWhenUnqualified,
                    limbsAreAssessedEveryVisit);

            // The coxofemoral rule only exists to stop the model duplicating what the server records for it. Where
            // nothing is recorded automatically there is nothing to keep out of its way, and saying a limb is filled
            // in afterwards would be a lie.
            string[] forelimbHints = limbsAreAssessedEveryVisit
                ? []
                :
                [
                    "Record a limb only where the dictation says something about it. Nothing is added afterwards, so a limb that was assessed and needed nothing has to be dictated as a mobilization to be recorded.",
                ];

            string[] rearLimbHints = limbsAreAssessedEveryVisit
                ?
                [
                    "Select a coxofemoral option only when the dictation names cranial or caudal for it. A bare \"coxofemoral mobilization\" with no direction needs nothing from you; both directions are recorded automatically.",
                    "Do not guess a side. If the dictation does not say left or right, leave both rear limbs empty; unmentioned limbs are filled in afterwards.",
                ]
                :
                [
                    "Record a limb only where the dictation says something about it. Nothing is added afterwards, so an unmentioned limb stays empty and a limb that was assessed and needed nothing has to be dictated as a mobilization to be recorded.",
                    "Do not guess a side. If the dictation does not say left or right, leave both rear limbs empty.",
                ];

            return new()
            {
                Species = species,
                Regions =
                [
                    // First, so it is the one carrying the intake fields: this is the request about the visit rather
                    // than about any part of the animal, which is where a patient name and a date belong.
                    ChiroVocabularyRegion.Of(
                        "visit and plan",
                        [],
                        [],
                        [
                            new(nameof(ExtendedOtherNotes), "Everything about the visit that is not an adjustment: reason for the visit, owner reports, behaviour, other conditions, and the plan"),
                        ]),
                    ChiroVocabularyRegion.Of(
                        "head and cervical",
                        [
                            new(nameof(HeadOcciput), "Head occiput", HeadOcciputChoices, nameof(HeadNotes)),
                            new(nameof(HeadTmj), "Head TMJ", HeadTmjChoices, nameof(HeadNotes)),
                            new(nameof(CervicalC1), "C1", CervicalC1Choices, nameof(CervicalNotes)),
                        ],
                        [
                            new(nameof(CervicalSpine), "Cervical spine", CervicalSpineRows, CervicalSpineColumns, nameof(CervicalNotes)),
                        ],
                        [
                            new(nameof(HeadNotes), "Head other notes"),
                            new(nameof(CervicalNotes), "Cervical other notes"),
                        ]),
                    ChiroVocabularyRegion.Of(
                        "thoracic and ribs",
                        [
                            new(nameof(Sternum), "Sternum", SternumChoices, nameof(ThoracicNotes)),
                        ],
                        [
                            new(nameof(ThoracicSpine), "Thoracic spine", ThoracicSpineRows, ThoracicSpineColumns, nameof(ThoracicNotes)),
                            new(nameof(Ribs), "Ribs", RibsRows, RibsColumns, nameof(ThoracicNotes)),
                        ],
                        [
                            new(nameof(ThoracicNotes), "Thoracic and rib other notes"),
                        ],
                        [
                            "A rib given a side but no direction is dorsal: \"rib 11 right\" is \"right dorsal\". Use cranial or caudal only where the dictation says so.",
                        ]),
                    ChiroVocabularyRegion.Of(
                        "lumbar, sacrum and pelvis",
                        [
                            new(nameof(PelvisTraction), "Pelvis traction", PelvisTractionChoices, nameof(PelvicNotes)),
                        ],
                        [
                            new(nameof(LumbarSpine), "Lumbar spine", LumbarSpineRows, LumbarSpineColumns, nameof(LumbarNotes)),
                            new(nameof(Sacrum), "Sacrum", SacrumRows, SacrumColumns, nameof(SacrumNotes)),
                            new(nameof(Pelvis), "Pelvis", PelvisRows, PelvisColumns, nameof(PelvicNotes)),
                        ],
                        [
                            new(nameof(LumbarNotes), "Lumbar other notes"),
                            new(nameof(SacrumNotes), "Sacrum other notes"),
                            new(nameof(PelvicNotes), "Pelvis other notes"),
                        ]),
                    ChiroVocabularyRegion.Of(
                        "forelimbs",
                        [
                            Limb(nameof(LeftForelimbScapula), "Left forelimb scapula", ForelimbScapulaChoices, nameof(LeftForelimbNotes)),
                            Limb(nameof(LeftForelimbHumorous), "Left forelimb humerus", ForelimbHumorousChoices, nameof(LeftForelimbNotes)),
                            Limb(nameof(LeftForelimbUlna), "Left forelimb ulna", ForelimbUlnaChoices, nameof(LeftForelimbNotes)),
                            Limb(nameof(LeftForelimbRadius), "Left forelimb radius", ForelimbRadiusChoices, nameof(LeftForelimbNotes)),
                            Limb(nameof(LeftForelimbCarpus), "Left forelimb carpus", ForelimbCarpusChoices, nameof(LeftForelimbNotes), expandsWhenUnqualified: true),
                            Limb(nameof(LeftForelimbMetatarsalsPhalanges), "Left forelimb digits", ForelimbMetatarsalsPhalangesChoices, nameof(LeftForelimbNotes)),
                            Limb(nameof(RightForelimbScapula), "Right forelimb scapula", ForelimbScapulaChoices, nameof(RightForelimbNotes)),
                            Limb(nameof(RightForelimbHumorous), "Right forelimb humerus", ForelimbHumorousChoices, nameof(RightForelimbNotes)),
                            Limb(nameof(RightForelimbUlna), "Right forelimb ulna", ForelimbUlnaChoices, nameof(RightForelimbNotes)),
                            Limb(nameof(RightForelimbRadius), "Right forelimb radius", ForelimbRadiusChoices, nameof(RightForelimbNotes)),
                            Limb(nameof(RightForelimbCarpus), "Right forelimb carpus", ForelimbCarpusChoices, nameof(RightForelimbNotes), expandsWhenUnqualified: true),
                            Limb(nameof(RightForelimbMetatarsalsPhalanges), "Right forelimb digits", ForelimbMetatarsalsPhalangesChoices, nameof(RightForelimbNotes)),
                        ],
                        [],
                        [
                            new(nameof(LeftForelimbNotes), "Left forelimb other notes"),
                            new(nameof(RightForelimbNotes), "Right forelimb other notes"),
                        ],
                        forelimbHints),
                    ChiroVocabularyRegion.Of(
                        "rear limbs and coccygeal",
                        [
                            Limb(nameof(LeftRearLimb), "Left rear limb", RearLimbChoices, nameof(LeftRearLimbNotes), RearLimbStandingSites),
                            Limb(nameof(RightRearLimb), "Right rear limb", RearLimbChoices, nameof(RightRearLimbNotes), RearLimbStandingSites),
                            new(nameof(Coccygeal), "Coccygeal", CoccygealChoices, nameof(CoccygealNotes)),
                        ],
                        [],
                        [
                            new(nameof(LeftRearLimbNotes), "Left rear limb other notes"),
                            new(nameof(RightRearLimbNotes), "Right rear limb other notes"),
                            new(nameof(CoccygealNotes), "Coccygeal other notes"),
                        ],
                        rearLimbHints),
                ],
            };
        }

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

        /// <summary>
        /// Prefills the form from a dictation. Deliberately does not submit: the draft is a starting point for the
        /// person who was in the room, not a record.
        /// </summary>
        public async Task<IActionResult> OnPostParseAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(this.Transcript))
            {
                this.ErrorMessage = "Paste a dictation transcript before parsing.";
                return this.Page();
            }

            try
            {
                ChiroParseResult result = await this.parser.ParseAsync(this.Vocabulary, this.Transcript, cancellationToken);
                this.ApplyParse(this.Vocabulary, result);
            }
            catch (Exception ex)
            {
                // Any parse failure has to leave a usable form behind; filling it in by hand is still the fallback.
                this.ErrorMessage = $"Could not parse the dictation, so the form is unchanged: {ex.Message}";
            }

            return this.Page();
        }

        public async Task<IActionResult> OnPostSubmitAsync(CancellationToken cancellationToken)
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

            ChiroSpecies species = this.Vocabulary.Species;

            try
            {
                ChiroInputCreated created = await this.NsClient.Chiro.SubmitAsync(species, input, cancellationToken);
                return this.RedirectToSubmitted(species, input, created);
            }
            catch (NsTcpWtfClientException ex)
            {
                this.ErrorMessage = ex.Message;
                return this.Page();
            }
        }
    }
}
