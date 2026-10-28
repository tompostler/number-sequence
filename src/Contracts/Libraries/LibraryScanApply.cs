namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// The choices made in the reconcile view. Everything else applying a session does needs no choice: a matched copy
    /// recorded here is verified, and a matched copy that was missing is restored.
    /// </summary>
    public sealed class LibraryScanApply
    {
        /// <summary>
        /// Matched copies recorded somewhere else, to move to the session's location.
        /// </summary>
        public List<long> MoveCopyIds { get; set; } = [];

        /// <summary>
        /// Matched copies on loan, to return: it was on the shelf, so it's back.
        /// </summary>
        public List<long> ReturnCopyIds { get; set; } = [];

        /// <summary>
        /// Copies from <see cref="LibraryScanReconcile.NotScanned"/>, to mark missing.
        /// </summary>
        public List<long> MissingCopyIds { get; set; } = [];
    }
}
