namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// One series for several items at once, each with its own number. For books added without their series, or a misspelled
    /// series: the name is typed once, not per book.
    /// </summary>
    public sealed class LibrarySeriesAssign
    {
        /// <summary>
        /// The series name, used as given. Blank takes the items out of any series, and clears their numbers too.
        /// </summary>
        public string Series { get; set; }

        public List<LibrarySeriesAssignItem> Items { get; set; } = [];
    }

    public sealed class LibrarySeriesAssignItem
    {
        public long ItemId { get; set; }

        /// <summary>
        /// The number in the series. Null for none, so send the current number to keep it.
        /// </summary>
        public decimal? Number { get; set; }
    }
}
