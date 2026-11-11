namespace number_sequence.Models
{
    /// <summary>
    /// Which feature an <see cref="EmailDocument"/> came from, and so which role can see it on the pdf status page.
    /// </summary>
    public enum EmailDocumentKind
    {
        /// <summary>Never written on purpose. A creation site that forgets to set it gets a row nobody can see, rather than one shown to the wrong role.</summary>
        Unknown,

        Chiro,
        Ledger,
    }
}
