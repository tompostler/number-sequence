namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// How a <see cref="LibraryScanEntry"/> matched against the library when it was scanned.
    /// </summary>
    public enum LibraryScanResolution
    {
        /// <summary>
        /// Matched a copy already recorded at the session's location.
        /// </summary>
        Expected,

        /// <summary>
        /// Matched a copy recorded at a different location, or at no location.
        /// </summary>
        Elsewhere,

        /// <summary>
        /// No copy in the library has this barcode, or every copy with it was already matched by an earlier scan in the
        /// session.
        /// </summary>
        NotInLibrary,

        /// <summary>
        /// Was <see cref="NotInLibrary"/>, and has since been added from the reconcile view as
        /// <see cref="LibraryScanEntry.ResolvedCopyId"/>. Applying the session records nothing more for it: adding the copy
        /// already recorded where it is.
        /// </summary>
        Added,
    }
}
