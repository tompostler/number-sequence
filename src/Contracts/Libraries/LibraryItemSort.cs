namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// The order items are listed in.
    /// </summary>
    public enum LibraryItemSort
    {
        /// <summary>
        /// The way a shelf is arranged: the first creator's last name, then the series (or the title, outside one), then
        /// the number in the series, then the title. A leading "The", "A" or "An" is ignored.
        /// </summary>
        Shelf,

        /// <summary>
        /// By title, ignoring a leading "The", "A" or "An".
        /// </summary>
        Title,

        /// <summary>
        /// Newest first, by when the item or any of its copies was added, so a new copy of an item already here counts.
        /// </summary>
        Added,
    }
}
