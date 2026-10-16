using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.Utilities
{
    /// <summary>
    /// When a copy's events happened, as opposed to when they were recorded. Used by both the item history and the gource
    /// export, so the two can't disagree.
    /// <para>
    /// The real date lives on the row that owns it: <see cref="LibraryCopy.AcquiredDate"/> for <see cref="LibraryCopyEventType.Acquired"/>,
    /// and the loan's <see cref="LibraryLoan.LoanedDate"/> and <see cref="LibraryLoan.ReturnedDate"/> for
    /// <see cref="LibraryCopyEventType.Loaned"/> and <see cref="LibraryCopyEventType.Returned"/>. It is read from there
    /// every time rather than copied onto the event. Events are append-only, so a copied date would go stale as soon as a
    /// purchase date was filled in later. Every other event, or one whose date isn't set, uses its
    /// <see cref="LibraryCopyEvent.CreatedDate"/>.
    /// </para>
    /// <para>
    /// Each time is then clamped up to at least the previous event's time, walking in recorded order. That keeps the
    /// result in recorded order whatever dates are entered: a copy can't move or be lent before it arrives, and a loan
    /// backdated past a move recorded before it lands at the move. The gource export depends on this, because each move's
    /// delete line names the path from the event's <see cref="LibraryCopyEvent.FromLocationId"/>, and that path only exists
    /// in gource's tree if the events before it came out first.
    /// </para>
    /// </summary>
    public static class LibraryEventTimes
    {
        /// <summary>
        /// One event and when it happened.
        /// </summary>
        /// <param name="IsDateOnly">
        /// The time came from a date with no time of day, and is midnight UTC of that date. Display it as a date, not
        /// converted to a local time, or it shows as the previous day anywhere west of UTC.
        /// </param>
        public readonly record struct Timed(LibraryCopyEvent Event, DateTimeOffset Time, bool IsDateOnly);

        /// <summary>
        /// Times for one copy's events, in recorded order (which is also time order).
        /// </summary>
        /// <param name="copy">The copy the events belong to.</param>
        /// <param name="events">That copy's events, in any order.</param>
        /// <param name="loans">That copy's loans. A loan that can't be found falls back to the event's recorded time.</param>
        public static List<Timed> Compute(LibraryCopy copy, IEnumerable<LibraryCopyEvent> events, IEnumerable<LibraryLoan> loans)
        {
            Dictionary<long, LibraryLoan> loansById = loans.ToDictionary(x => x.Id);

            List<Timed> result = [];
            DateTimeOffset? previous = null;
            foreach (LibraryCopyEvent copyEvent in events.OrderBy(x => x.CreatedDate).ThenBy(x => x.Id))
            {
                LibraryLoan loan = copyEvent.LoanId.HasValue ? loansById.GetValueOrDefault(copyEvent.LoanId.Value) : null;
                DateOnly? ownedDate = copyEvent.EventType switch
                {
                    LibraryCopyEventType.Acquired => copy.AcquiredDate,
                    LibraryCopyEventType.Loaned => loan?.LoanedDate,
                    LibraryCopyEventType.Returned => loan?.ReturnedDate,
                    _ => null,
                };

                DateTimeOffset time = ownedDate.HasValue
                    ? new DateTimeOffset(ownedDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
                    : copyEvent.CreatedDate;
                bool isDateOnly = ownedDate.HasValue;

                if (previous.HasValue && time < previous.Value)
                {
                    time = previous.Value;
                    isDateOnly = false;
                }

                result.Add(new(copyEvent, time, isDateOnly));
                previous = time;
            }

            return result;
        }
    }
}
