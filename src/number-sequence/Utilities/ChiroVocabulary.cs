using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Utilities
{
    /// <summary>
    /// The complete set of values a species form can hold, described in one place so that the dictation parser and
    /// the rendered checkboxes cannot drift apart.
    /// <para>
    /// A species form owns its own choices, so it builds this from the same static arrays the razor page renders
    /// from. Nothing here is hand written; if it were, it would be a second source of truth and the whole point of
    /// generating the model's schema from the form would be lost.
    /// </para>
    /// </summary>
    public sealed record ChiroVocabulary
    {
        public required ChiroSpecies Species { get; init; }

        /// <summary>
        /// The form split into the chunks a single parse request covers. One request for the whole form compiles a
        /// grammar the api rejects as too large, so the split is a hard requirement rather than a tuning choice.
        /// Regions are parsed concurrently, which is also what keeps the round trip short.
        /// </summary>
        public required IReadOnlyList<ChiroVocabularyRegion> Regions { get; init; }

        /// <summary>
        /// Terminology and dictation habits particular to this species, given to the model on every region. Lives
        /// here rather than in the parser because it is the same kind of knowledge as the choice arrays: what this
        /// form's words mean. A shared prompt would have to be kept in step with two species by hand.
        /// </summary>
        public IReadOnlyList<string> Glossary { get; init; } = [];

        public IEnumerable<ChiroVocabularyGroup> Groups => this.Regions.SelectMany(x => x.Groups);

        public IEnumerable<ChiroVocabularyGrid> Grids => this.Regions.SelectMany(x => x.Grids);

        public IEnumerable<ChiroVocabularyNote> Notes => this.Regions.SelectMany(x => x.Notes);
    }

    /// <summary>
    /// One parse request's worth of the form. Keep a region's questions to things a dictation would talk about
    /// together, so the model is never asked about an area the surrounding words say nothing about.
    /// </summary>
    /// <param name="Name">Used in logs to identify which request is which.</param>
    /// <param name="Hints">
    /// Rules that only make sense for this region's questions, such as what to do when the dictation names a joint
    /// without the qualifier the form requires. Kept off the shared prompt so a rule about one region cannot
    /// confuse the four requests it does not apply to.
    /// </param>
    public sealed record ChiroVocabularyRegion(
        string Name,
        IReadOnlyList<ChiroVocabularyGroup> Groups,
        IReadOnlyList<ChiroVocabularyGrid> Grids,
        IReadOnlyList<ChiroVocabularyNote> Notes,
        IReadOnlyList<string> Hints)
    {
        public static ChiroVocabularyRegion Of(
            string name,
            ChiroVocabularyGroup[] groups,
            ChiroVocabularyGrid[] grids,
            ChiroVocabularyNote[] notes,
            string[] hints = null)
            => new(name, groups, grids, notes, hints ?? []);
    }

    /// <param name="Name">The bound property name, which is also the key the model answers under.</param>
    /// <param name="Label">Human phrasing, given to the model so it knows what the field is.</param>
    /// <param name="Choices">The only values that may be selected.</param>
    /// <param name="NotesName">The note field unmappable text for this question belongs in.</param>
    /// <param name="StandingSites">
    /// Sites assessed at every visit, named by the text their options share, eg "Coxo-Femoral". Their mobilization
    /// options are recorded unless the dictation named one of that site's own options, and they are recorded even
    /// when the same question carries findings for other sites. A question covering several joints needs this
    /// because an adjustment to one joint says nothing about whether the others were looked at.
    /// </param>
    /// <param name="ExpandsWhenUnqualified">
    /// Whether naming this finding without the qualifier its options require means all of them. True of a carpus,
    /// where "carpal hypomobility" with no anterior or accessory means both. Emphatically not true everywhere: a rib
    /// named without a direction means one of dorsal, cranial or caudal that the dictation elided, not all three,
    /// so this is opted into per question rather than stated as a general rule.
    /// </param>
    /// <param name="DefaultsToMobilization">
    /// Whether a question the dictation never mentions means its mobilization options, ie that the site is assessed
    /// at every visit whether or not it is dictated. True nearly everywhere. False on the feline limbs, where a cat
    /// is only worked on where it tolerates being worked on, so an unmentioned limb means nothing was done rather
    /// than that nothing was needed.
    /// </param>
    public sealed record ChiroVocabularyGroup(
        string Name,
        string Label,
        string[] Choices,
        string NotesName,
        string[] StandingSites = null,
        bool ExpandsWhenUnqualified = false,
        bool DefaultsToMobilization = true)
    {
        public string[] StandingSites { get; init; } = StandingSites ?? [];
    }

    /// <param name="Name">The bound property name, which is also the key the model answers under.</param>
    /// <param name="Label">Human phrasing, given to the model so it knows what the field is.</param>
    /// <param name="Rows">Answered independently; the model lists only the ones the dictation mentions.</param>
    /// <param name="Columns">The only values that may be selected for any row.</param>
    /// <param name="NotesName">The note field unmappable text for this question belongs in.</param>
    public sealed record ChiroVocabularyGrid(string Name, string Label, string[] Rows, string[] Columns, string NotesName);

    /// <param name="Name">The bound property name, which is also the key the model answers under.</param>
    /// <param name="Label">Human phrasing, given to the model so it knows what the field is.</param>
    public sealed record ChiroVocabularyNote(string Name, string Label);
}
