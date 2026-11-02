namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// What a fresh lookup of a saved item has, next to what's saved: each field where the lookup has a different, non-empty
    /// value. Nothing is changed by getting one; take fields with <see cref="LibraryItemRefreshApply"/>.
    /// </summary>
    public sealed class LibraryItemRefresh
    {
        public long ItemId { get; set; }

        /// <summary>
        /// The copy whose ISBN was looked up, whose format, publisher and year are offered too. Null when the item was looked
        /// up by a lookup id instead.
        /// </summary>
        public long? CopyId { get; set; }

        /// <summary>
        /// Copies with an ISBN, any of which could be looked up instead (by passing its id).
        /// </summary>
        public List<long> IsbnCopyIds { get; set; } = [];

        /// <summary>
        /// What was looked up, such as "ISBN 9781250251466", for showing.
        /// </summary>
        public string LookedUpBy { get; set; }

        /// <summary>
        /// Where the offered values come from; one of <see cref="LibraryLookupSources"/>. Null when nothing was found.
        /// </summary>
        public string Source { get; set; }

        public string SourceUrl { get; set; }

        /// <summary>
        /// Why there's nothing to compare: no way to look the item up, or the lookup found nothing. Null when there is.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// The fields that would change, in a fixed order.
        /// </summary>
        public List<LibraryItemRefreshChange> Changes { get; set; } = [];

        /// <summary>
        /// Labels of the fields the lookup agrees with.
        /// </summary>
        public List<string> Same { get; set; } = [];

        /// <summary>
        /// The lookup's cover, offered as <see cref="LibraryItemRefreshFields.Cover"/>; empty when it has none. Whether it
        /// differs from the saved one isn't known.
        /// </summary>
        public List<string> CoverUrls { get; set; } = [];
    }

    /// <summary>
    /// One field a refresh would change.
    /// </summary>
    public sealed class LibraryItemRefreshChange
    {
        /// <summary>
        /// One of <see cref="LibraryItemRefreshFields"/>.
        /// </summary>
        public string Field { get; set; }

        public string Label { get; set; }

        /// <summary>
        /// What's saved, as text; null when empty.
        /// </summary>
        public string Current { get; set; }

        /// <summary>
        /// What the lookup has, as text. For <see cref="LibraryItemRefreshFields.Attributes"/> and
        /// <see cref="LibraryItemRefreshFields.ExternalIds"/>, what's saved with the lookup's entries added or replacing.
        /// </summary>
        public string Offered { get; set; }
    }

    /// <summary>
    /// The fields to take from a refresh. A value is taken as given, so it can be changed first (a series respelled to
    /// match the rest); blank clears the field. Attributes, external ids and the cover take no value: they're the lookup's.
    /// </summary>
    public sealed class LibraryItemRefreshApply
    {
        /// <summary>
        /// The copy to look up by, as in the refresh being applied.
        /// </summary>
        public long? CopyId { get; set; }

        /// <summary>
        /// Field (from <see cref="LibraryItemRefreshFields"/>) to value.
        /// </summary>
        public Dictionary<string, string> Take { get; set; } = [];
    }

    /// <summary>
    /// What applying a refresh changed.
    /// </summary>
    public sealed class LibraryItemRefreshApplied
    {
        /// <summary>
        /// Labels of the fields taken.
        /// </summary>
        public List<string> Taken { get; set; } = [];

        /// <summary>
        /// Why the cover wasn't taken when it was asked for, such as the image no longer fetching. Everything else was saved.
        /// </summary>
        public string CoverNote { get; set; }
    }

    /// <summary>
    /// The fields a refresh compares.
    /// </summary>
    public static class LibraryItemRefreshFields
    {
        public const string Title = "title";
        public const string Subtitle = "subtitle";
        public const string Creators = "creators";
        public const string Series = "series";
        public const string SeriesNumber = "seriesNumber";
        public const string Year = "year";
        public const string Summary = "summary";
        public const string Attributes = "attributes";
        public const string ExternalIds = "externalIds";
        public const string Cover = "cover";
        public const string Format = "format";
        public const string Publisher = "publisher";
        public const string PublishedYear = "publishedYear";
    }
}
