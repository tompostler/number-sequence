using FluentAssertions;
using number_sequence.Utilities;

namespace number_sequence.UnitTests.UtilityTests
{
    [TestClass]
    public sealed class LibrarySeriesTests
    {
        [TestMethod]
        [DataRow("Wheel of Time", "The Wheel of Time")]
        [DataRow("the wheel of time", "The Wheel of Time")]
        [DataRow("Dark Tower", "The Dark Tower")]
        [DataRow("Discworld", "Discworld")]
        public void MatchExistingUsesTheLibrarysSpelling(string looked, string expected)
        {
            // Act
            string matched = LibrarySeries.MatchExisting(looked, ["The Wheel of Time", "The Dark Tower"]);

            // Assert
            _ = matched.Should().Be(expected);
        }
    }
}
