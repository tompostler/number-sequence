namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// Keys of <see cref="LibraryItem.Attributes"/> that lookups fill in. Other keys are allowed.
    /// </summary>
    public static class LibraryAttributes
    {
        /// <summary>
        /// Page count of a book.
        /// </summary>
        public const string Pages = "pages";

        /// <summary>
        /// Running time of a movie, in minutes.
        /// </summary>
        public const string RuntimeMinutes = "runtimeMinutes";

        /// <summary>
        /// Number of seasons of a TV series.
        /// </summary>
        public const string Seasons = "seasons";

        /// <summary>
        /// Genres, comma separated.
        /// </summary>
        public const string Genres = "genres";

        /// <summary>
        /// Player count of a game, such as <c>2–4</c>.
        /// </summary>
        public const string Players = "players";

        /// <summary>
        /// Playing time of a game, in minutes.
        /// </summary>
        public const string PlayTimeMinutes = "playTimeMinutes";

        /// <summary>
        /// Minimum age of a game.
        /// </summary>
        public const string MinAge = "minAge";
    }
}
