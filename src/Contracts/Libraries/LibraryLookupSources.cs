namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// Values of <see cref="LibraryLookupResult.Source"/>.
    /// </summary>
    public static class LibraryLookupSources
    {
        /// <summary>
        /// Open Library. The source id is a work id, such as <c>OL45883W</c>.
        /// </summary>
        public const string OpenLibrary = "openlibrary";

        /// <summary>
        /// Google Books, the fallback for an ISBN Open Library doesn't have. Only returned by an ISBN lookup.
        /// </summary>
        public const string GoogleBooks = "googlebooks";

        /// <summary>
        /// A movie at TMDB.
        /// </summary>
        public const string TmdbMovie = "tmdb-movie";

        /// <summary>
        /// A TV series at TMDB.
        /// </summary>
        public const string TmdbTv = "tmdb-tv";

        /// <summary>
        /// A board game or expansion at BoardGameGeek.
        /// </summary>
        public const string BoardGameGeek = "bgg";
    }
}
