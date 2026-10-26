namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// A cover to store for an item: either the image itself in <see cref="Data"/>, or <see cref="SourceUrls"/> from a
    /// <see cref="LibraryLookupResult"/> for the server to fetch. The stored cover is served at
    /// <c>library/libraries/{libraryId}/items/{itemId}/cover</c>.
    /// </summary>
    public sealed class LibraryCover
    {
        /// <summary>
        /// A JPEG, PNG, GIF or WebP image. The type is read from the bytes, not from the request.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// <see cref="LibraryLookupResult.CoverUrls"/>, tried in order until one fetches. Only lookup provider hosts are
        /// fetched from.
        /// </summary>
        public List<string> SourceUrls { get; set; }
    }
}
