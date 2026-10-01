namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// The level of access an account has to a library. Ordered so that a higher value includes everything a lower one allows.
    /// </summary>
    public enum LibraryPermission
    {
        /// <summary>
        /// Can browse and search the library.
        /// </summary>
        Viewer,

        /// <summary>
        /// Can also add, change, move, and loan items and copies, and manage locations.
        /// </summary>
        Editor,

        /// <summary>
        /// Can also manage shares, and rename or delete the library. Never stored on a <see cref="LibraryShare"/>; only the
        /// <see cref="Library.AccountName"/> is the owner.
        /// </summary>
        Owner,
    }
}
