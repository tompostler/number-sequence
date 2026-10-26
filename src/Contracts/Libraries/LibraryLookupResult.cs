namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// What an outside service knows about a book, movie or game, used to fill in the add form. Nothing is saved by a lookup.
    /// The item fields go on a <see cref="LibraryItem"/>, and the edition fields on its first <see cref="LibraryCopy"/>.
    /// <para>
    /// A search returns candidates to choose between, which can leave out fields such as <see cref="Creators"/> or
    /// <see cref="Summary"/>. Fetch the chosen one by <see cref="Source"/> and <see cref="SourceId"/> for the full record.
    /// </para>
    /// </summary>
    public sealed class LibraryLookupResult
    {
        /// <summary>
        /// Where this came from; one of <see cref="LibraryLookupSources"/>.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The record's id at <see cref="Source"/>, such as an Open Library work id or a TMDB movie id.
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// The record's page at <see cref="Source"/>, for a person to check it's the right one.
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// See <see cref="LibraryItem.MediaType"/>.
        /// </summary>
        public LibraryMediaType MediaType { get; set; }

        /// <summary>
        /// See <see cref="LibraryItem.Title"/>.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// See <see cref="LibraryItem.Subtitle"/>.
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// See <see cref="LibraryItem.Creators"/>.
        /// </summary>
        public string Creators { get; set; }

        /// <summary>
        /// See <see cref="LibraryItem.Series"/>.
        /// </summary>
        public string Series { get; set; }

        /// <summary>
        /// See <see cref="LibraryItem.SeriesNumber"/>.
        /// </summary>
        public decimal? SeriesNumber { get; set; }

        /// <summary>
        /// See <see cref="LibraryItem.Year"/>. Left out rather than guessed when the source only knows an edition's year.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// See <see cref="LibraryItem.Summary"/>.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// See <see cref="LibraryItem.ExternalIds"/>. Keys are from <see cref="LibraryExternalIds"/>.
        /// </summary>
        public Dictionary<string, string> ExternalIds { get; set; }

        /// <summary>
        /// See <see cref="LibraryItem.Attributes"/>. Keys are from <see cref="LibraryAttributes"/>.
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// Cover images at the source, best first. Later ones are smaller, for when an earlier one is missing or too large to
        /// store. Pass them to the item's cover upload rather than linking to them.
        /// </summary>
        public List<string> CoverUrls { get; set; }

        /// <summary>
        /// See <see cref="LibraryCopy.Barcode"/>. Only set by a barcode lookup.
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// See <see cref="LibraryCopy.Format"/>.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// See <see cref="LibraryCopy.Publisher"/>.
        /// </summary>
        public string Publisher { get; set; }

        /// <summary>
        /// See <see cref="LibraryCopy.PublishedYear"/>.
        /// </summary>
        public int? PublishedYear { get; set; }
    }
}
