namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// Keys of <see cref="LibraryItem.ExternalIds"/>. An item sharing any of these with a lookup result is offered as the
    /// item to add a copy to, so the keys are part of how duplicates are found and must not be renamed.
    /// </summary>
    public static class LibraryExternalIds
    {
        /// <summary>
        /// The Open Library work, such as <c>OL45804W</c>. Every edition of a book shares it, which is how a paperback finds
        /// the hardcover's item.
        /// </summary>
        public const string OpenLibraryWork = "openLibraryWork";

        /// <summary>
        /// The Open Library edition, such as <c>OL7353617M</c>.
        /// </summary>
        public const string OpenLibraryEdition = "openLibraryEdition";

        /// <summary>
        /// The Google Books volume id.
        /// </summary>
        public const string GoogleBooks = "googleBooks";

        /// <summary>
        /// The TMDB movie id.
        /// </summary>
        public const string TmdbMovie = "tmdbMovie";

        /// <summary>
        /// The TMDB TV series id.
        /// </summary>
        public const string TmdbTv = "tmdbTv";

        /// <summary>
        /// The IMDb id, such as <c>tt0133093</c>.
        /// </summary>
        public const string Imdb = "imdb";

        /// <summary>
        /// The BoardGameGeek thing id.
        /// </summary>
        public const string BoardGameGeek = "bgg";
    }
}
