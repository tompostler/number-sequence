using number_sequence.DataAccess;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Utilities
{
    /// <summary>
    /// Everything that varies between chiro species.
    /// The orchestration, the pdf generation, and the submission validation are all shared and key off of this,
    /// so adding a species is a new entry here plus a <c>chiro-{species}-diagram.png</c> resource, a form page, and a pdf template row.
    /// </summary>
    public sealed record ChiroSpeciesDefinition
    {
        public required ChiroSpecies Species { get; init; }

        /// <summary>One of the <see cref="NsStorage.C.PT"/> values.</summary>
        public required string TemplateId { get; init; }

        /// <summary>Used in the email subject.</summary>
        public required string DisplayName { get; init; }

        // The pdf generation indexes into these arrays without null checks, so submissions are rejected before they are recorded if the lengths do not line up.
        public required int ThoracicCount { get; init; }
        public required int RibsCount { get; init; }
        public required int LumbarCount { get; init; }

        /// <summary>0 when the species does not have intertransverse data.</summary>
        public required int LumbarIntertransverseCount { get; init; }

        private static readonly Dictionary<ChiroSpecies, ChiroSpeciesDefinition> Definitions =
            new ChiroSpeciesDefinition[]
            {
                new()
                {
                    Species = ChiroSpecies.Canine,
                    TemplateId = NsStorage.C.PT.ChiroCanine,
                    DisplayName = "Canine",
                    ThoracicCount = 13,
                    RibsCount = 13,
                    LumbarCount = 7,
                    LumbarIntertransverseCount = 0,
                },
                new()
                {
                    Species = ChiroSpecies.Equine,
                    TemplateId = NsStorage.C.PT.ChiroEquine,
                    DisplayName = "Equine",
                    ThoracicCount = 18,
                    RibsCount = 18,
                    LumbarCount = 6,
                    LumbarIntertransverseCount = 3,
                },
            }.ToDictionary(x => x.Species);

        public static bool TryGet(ChiroSpecies species, out ChiroSpeciesDefinition definition)
            => Definitions.TryGetValue(species, out definition);

        public static ChiroSpeciesDefinition Get(ChiroSpecies species)
            => TryGet(species, out ChiroSpeciesDefinition definition)
                ? definition
                : throw new ArgumentOutOfRangeException(nameof(species), species, "No chiro species definition is registered.");
    }
}
