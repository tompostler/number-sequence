namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// What applying a <see cref="LibraryScanSession"/> could do: its entries, each with its
    /// <see cref="LibraryScanEntry.ResolvedCopy"/>, and the copies recorded at the session's location that no entry matched.
    /// </summary>
    public sealed class LibraryScanReconcile
    {
        public LibraryScanSession Session { get; set; }

        /// <summary>
        /// Active copies recorded at the session's location that no entry matched, each with its item and open loan. A copy
        /// on loan is listed but isn't expected on the shelf, so it can't be marked missing.
        /// </summary>
        public List<LibraryCopy> NotScanned { get; set; }
    }
}
