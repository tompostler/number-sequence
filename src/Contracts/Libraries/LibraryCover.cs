namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// A cover to store for an item: <see cref="SourceUrls"/> from a <see cref="LibraryLookupResult"/> for the server to
    /// fetch. Covers only come from lookups; there are no uploads. The stored cover is served at
    /// <c>library/libraries/{libraryId}/items/{itemId}/cover</c>.
    /// </summary>
    public sealed class LibraryCover
    {
        /// <summary>
        /// <see cref="LibraryLookupResult.CoverUrls"/>, tried in order until one fetches. Only lookup provider hosts are
        /// fetched from.
        /// </summary>
        public List<string> SourceUrls { get; set; }
    }
}
