namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// Whether a <see cref="LibraryCopy"/> is still part of the library. A copy on loan is still <see cref="Active"/>; loans
    /// are tracked by <see cref="LibraryLoan"/>.
    /// </summary>
    public enum LibraryCopyStatus
    {
        /// <summary>
        /// The copy is part of the library.
        /// </summary>
        Active,

        /// <summary>
        /// The copy was expected somewhere and was not found.
        /// </summary>
        Missing,

        /// <summary>
        /// The copy was sold, given away, or otherwise removed. Kept for history.
        /// </summary>
        Disposed,
    }
}
