using FluentAssertions;
using number_sequence.Utilities;

namespace number_sequence.UnitTests.UtilityTests
{
    [TestClass]
    public sealed class LibrarySeriesTests
    {
        [TestMethod]
        [DataRow("Discworld (3)", "Discworld", "3")]
        [DataRow("Harry Potter #1", "Harry Potter", "1")]
        [DataRow("Discworld ; 3", "Discworld", "3")]
        [DataRow("Wheel of Time (11)", "Wheel of Time", "11")]
        [DataRow("The Wheel of Time, Book Ten;", "The Wheel of Time", "10")]
        [DataRow("The Dark Tower VII", "The Dark Tower", "7")]
        [DataRow("The Dark Tower, IV", "The Dark Tower", "4")]
        [DataRow("Foundation, vol. 2", "Foundation", "2")]
        [DataRow("Earthsea 1.5", "Earthsea", "1.5")]
        [DataRow("Wheel of Time", "Wheel of Time", null)]
        [DataRow("A Discworld novel", "A Discworld novel", null)]
        public void ParseFieldFindsNameAndNumber(string field, string name, string number)
        {
            // Act
            LibrarySeriesPart? parsed = LibrarySeries.ParseField(field);

            // Assert
            _ = parsed.Should().Be(new LibrarySeriesPart(name, number == null ? null : decimal.Parse(number, System.Globalization.CultureInfo.InvariantCulture)));
        }

        [TestMethod]
        [DataRow("The Great Hunt (The Wheel of Time Book 2)", "The Great Hunt", "The Wheel of Time", 2)]
        [DataRow("Knife of Dreams (The Wheel of Time, Book 11)", "Knife of Dreams", "The Wheel of Time", 11)]
        [DataRow("(The Wheel of Time, Book 6)", null, "The Wheel of Time", 6)]
        [DataRow("Book Two of 'The Wheel of Time'", null, "The Wheel of Time", 2)]
        [DataRow("Book One of The Wheel of Time", null, "The Wheel of Time", 1)]
        [DataRow("Book Three of the Riftwar Saga", null, "The Riftwar Saga", 3)]
        [DataRow("Guards! Guards! [Discworld #8]", "Guards! Guards!", "Discworld", 8)]
        public void ExtractTakesSeriesOutOfTitle(string text, string rest, string name, int number)
        {
            // Act
            (string remaining, LibrarySeriesPart? series) = LibrarySeries.Extract(text);

            // Assert
            _ = remaining.Should().Be(rest);
            _ = series.Should().Be(new LibrarySeriesPart(name, number));
        }

        [TestMethod]
        [DataRow("Fahrenheit 451", DisplayName = "A title ending in a number")]
        [DataRow("The Novel (Wheel of Time)", DisplayName = "A bracketed series with no number")]
        [DataRow("Rocky II", DisplayName = "A bare numeral")]
        [DataRow("Book of the Dead", DisplayName = "A title starting with Book")]
        [DataRow("Dune (Movie Tie-In)", DisplayName = "A bracketed note")]
        public void ExtractLeavesOrdinaryTitlesAlone(string text)
        {
            // Act
            (string remaining, LibrarySeriesPart? series) = LibrarySeries.Extract(text);

            // Assert
            _ = remaining.Should().Be(text);
            _ = series.Should().BeNull();
        }

        [TestMethod]
        public void IsSeriesTitleNeedsTheKnownSeries()
        {
            // Act and assert
            _ = LibrarySeries.IsSeriesTitle("The Dark Tower III", new("The Dark Tower", 3), out decimal? number).Should().BeTrue();
            _ = number.Should().Be(3);
            _ = LibrarySeries.IsSeriesTitle("Dark Tower III", new("The Dark Tower", null), out _).Should().BeTrue();
            _ = LibrarySeries.IsSeriesTitle("Rocky II", new("The Dark Tower", 3), out _).Should().BeFalse();
            _ = LibrarySeries.IsSeriesTitle("The Dark Tower", new("The Dark Tower", 7), out _).Should().BeFalse();
        }

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
