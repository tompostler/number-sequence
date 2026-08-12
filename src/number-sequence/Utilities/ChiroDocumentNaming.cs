using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Utilities
{
    /// <summary>
    /// What a chiro record's pdf ends up called, derived from the record alone.
    /// <para>
    /// Shared rather than computed where it is needed, because the submitting page shows the doctor the name before
    /// the pdf exists and the generation activity is what actually names it. Two copies of this would be two
    /// filenames the day either one changed.
    /// </para>
    /// </summary>
    public static class ChiroDocumentNaming
    {
        /// <summary>
        /// The owner as it appears on the pdf and in the filename. The clinic prefix is part of the name rather than
        /// a separate field so that a clinic's records sort together.
        /// </summary>
        public static string OwnerName(ChiroInput input)
            => string.IsNullOrWhiteSpace(input.ClinicAbbreviation)
                ? input.OwnerName
                : input.ClinicAbbreviation + " - " + input.OwnerName;

        /// <summary>
        /// The attachment name, without the .pdf that storage appends. Anything that is not a letter, digit, dash or
        /// underscore becomes a dash, which is what keeps a name with a slash or a quote in it from breaking the
        /// blob path.
        /// </summary>
        public static string AttachmentName(ChiroInput input)
            => new(
                $"{input.DateOfService:yyyy-MM-dd}_{OwnerName(input)}_{input.PatientName}"
                    .Select(x => char.IsLetterOrDigit(x) || x == '-' || x == '_' ? x : '-')
                    .ToArray());
    }
}
