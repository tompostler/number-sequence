namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// The kind of thing a <see cref="LibraryItem"/> is. Stored as a string, so adding a value needs no migration.
    /// </summary>
    public enum LibraryMediaType
    {
        /// <summary>
        /// Anything not covered by a more specific type, such as a tool or other loanable item.
        /// </summary>
        Other,

        /// <summary>
        /// A book. Copies are usually identified by ISBN.
        /// </summary>
        Book,

        /// <summary>
        /// A movie or tv show on physical media.
        /// </summary>
        Movie,

        /// <summary>
        /// A board, card, or tabletop game.
        /// </summary>
        BoardGame,
    }
}
