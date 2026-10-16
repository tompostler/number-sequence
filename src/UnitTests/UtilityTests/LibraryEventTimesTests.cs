using FluentAssertions;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.UnitTests.UtilityTests
{
    [TestClass]
    public sealed class LibraryEventTimesTests
    {
        private static readonly DateTimeOffset ScanDay = new(2026, 10, 1, 14, 0, 0, TimeSpan.Zero);

        private static LibraryCopyEvent Event(long id, LibraryCopyEventType type, DateTimeOffset recorded, long? loanId = null)
            => new() { Id = id, EventType = type, CreatedDate = recorded, LoanId = loanId };

        [TestMethod]
        public void AcquiredUsesTheCopysAcquiredDate()
        {
            // Arrange
            LibraryCopy copy = new() { AcquiredDate = new DateOnly(2016, 5, 3) };
            LibraryCopyEvent[] events = [Event(1, LibraryCopyEventType.Acquired, ScanDay)];

            // Act
            List<LibraryEventTimes.Timed> times = LibraryEventTimes.Compute(copy, events, []);

            // Assert
            _ = times.Single().Time.Should().Be(new DateTimeOffset(2016, 5, 3, 0, 0, 0, TimeSpan.Zero));
            _ = times.Single().IsDateOnly.Should().BeTrue();
        }

        [TestMethod]
        public void WithoutADateEventsUseWhenTheyWereRecorded()
        {
            // Arrange
            LibraryCopy copy = new();
            LibraryCopyEvent[] events =
            [
                Event(2, LibraryCopyEventType.Moved, ScanDay.AddHours(1)),
                Event(1, LibraryCopyEventType.Acquired, ScanDay),
            ];

            // Act
            List<LibraryEventTimes.Timed> times = LibraryEventTimes.Compute(copy, events, []);

            // Assert
            _ = times.Select(x => x.Event.Id).Should().Equal(1, 2);
            _ = times.Select(x => x.Time).Should().Equal(ScanDay, ScanDay.AddHours(1));
            _ = times.Should().OnlyContain(x => !x.IsDateOnly);
        }

        [TestMethod]
        public void LoansUseTheLoansDates()
        {
            // Arrange
            LibraryCopy copy = new() { AcquiredDate = new DateOnly(2016, 5, 3) };
            LibraryLoan loan = new() { Id = 7, LoanedDate = new DateOnly(2026, 3, 10), ReturnedDate = new DateOnly(2026, 4, 2) };
            LibraryCopyEvent[] events =
            [
                Event(1, LibraryCopyEventType.Acquired, ScanDay),
                Event(2, LibraryCopyEventType.Loaned, ScanDay.AddMinutes(1), loanId: 7),
                Event(3, LibraryCopyEventType.Returned, ScanDay.AddMinutes(2), loanId: 7),
            ];

            // Act
            List<LibraryEventTimes.Timed> times = LibraryEventTimes.Compute(copy, events, [loan]);

            // Assert
            _ = times.Select(x => x.Time).Should().Equal(
                new DateTimeOffset(2016, 5, 3, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 3, 10, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 4, 2, 0, 0, 0, TimeSpan.Zero));
        }

        [TestMethod]
        public void ABackdatedLoanLandsAtTheMoveRecordedBeforeIt()
        {
            // Arrange: scanned in, moved an hour later, then a loan from last March recorded after both.
            LibraryCopy copy = new() { AcquiredDate = new DateOnly(2016, 5, 3) };
            LibraryLoan loan = new() { Id = 7, LoanedDate = new DateOnly(2026, 3, 10) };
            LibraryCopyEvent[] events =
            [
                Event(1, LibraryCopyEventType.Acquired, ScanDay),
                Event(2, LibraryCopyEventType.Moved, ScanDay.AddHours(1)),
                Event(3, LibraryCopyEventType.Loaned, ScanDay.AddHours(2), loanId: 7),
            ];

            // Act
            List<LibraryEventTimes.Timed> times = LibraryEventTimes.Compute(copy, events, [loan]);

            // Assert
            _ = times.Select(x => x.Event.Id).Should().Equal(1, 2, 3);
            _ = times[2].Time.Should().Be(ScanDay.AddHours(1));
            _ = times[2].IsDateOnly.Should().BeFalse();
        }

        [TestMethod]
        public void NothingHappensBeforeAnAcquisitionDatedAfterIt()
        {
            // Arrange: a purchase date corrected to later than a scan that was already recorded.
            LibraryCopy copy = new() { AcquiredDate = new DateOnly(2026, 12, 25) };
            LibraryCopyEvent[] events =
            [
                Event(1, LibraryCopyEventType.Acquired, ScanDay),
                Event(2, LibraryCopyEventType.Verified, ScanDay.AddDays(1)),
            ];

            // Act
            List<LibraryEventTimes.Timed> times = LibraryEventTimes.Compute(copy, events, []);

            // Assert
            DateTimeOffset christmas = new(2026, 12, 25, 0, 0, 0, TimeSpan.Zero);
            _ = times.Select(x => x.Time).Should().Equal(christmas, christmas);
        }

        [TestMethod]
        public void AMissingLoanFallsBackToWhenTheEventWasRecorded()
        {
            // Arrange
            LibraryCopy copy = new();
            LibraryCopyEvent[] events =
            [
                Event(1, LibraryCopyEventType.Acquired, ScanDay),
                Event(2, LibraryCopyEventType.Loaned, ScanDay.AddHours(1), loanId: 99),
            ];

            // Act
            List<LibraryEventTimes.Timed> times = LibraryEventTimes.Compute(copy, events, []);

            // Assert
            _ = times[1].Time.Should().Be(ScanDay.AddHours(1));
        }
    }
}
