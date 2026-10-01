namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// What happened to a copy in a <see cref="LibraryCopyEvent"/>.
    /// </summary>
    public enum LibraryCopyEventType
    {
        /// <summary>
        /// The copy was added to the library.
        /// </summary>
        Acquired,

        /// <summary>
        /// The copy changed location.
        /// </summary>
        Moved,

        /// <summary>
        /// The copy was lent out.
        /// </summary>
        Loaned,

        /// <summary>
        /// The copy came back from a loan.
        /// </summary>
        Returned,

        /// <summary>
        /// The copy was scanned where it was expected to be.
        /// </summary>
        Verified,

        /// <summary>
        /// The copy was marked as missing.
        /// </summary>
        MarkedMissing,

        /// <summary>
        /// The copy was removed from the library.
        /// </summary>
        Disposed,

        /// <summary>
        /// The copy was returned to <see cref="LibraryCopyStatus.Active"/> after being missing or disposed.
        /// </summary>
        Restored,
    }
}
