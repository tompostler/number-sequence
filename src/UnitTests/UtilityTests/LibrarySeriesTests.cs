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
        [DataRow("H.i.v.e -- 3", "H.i.v.e", "3")]
        [DataRow("H.I.V.E -- [#5]", "H.I.V.E", "5")]
        [DataRow("H.I.V.E -- [6]", "H.I.V.E", "6")]
        [DataRow("Spider-Man 2", "Spider-Man", "2")]
        [DataRow("H.I.V.E --", "H.I.V.E", null)]
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
        [DataRow("H.I.V.E.", DisplayName = "A trailing period")]
        [DataRow("'Salem's Lot", DisplayName = "A leading apostrophe")]
        public void ExtractLeavesOrdinaryTitlesAlone(string text)
        {
            // Act
            (string remaining, LibrarySeriesPart? series) = LibrarySeries.Extract(text);

            // Assert
            _ = remaining.Should().Be(text);
            _ = series.Should().BeNull();
        }

        [TestMethod]
        public void IsSeriesLabelNeedsTheKnownSeriesAndASubtitle()
        {
            // Act and assert
            _ = LibrarySeries.IsSeriesLabel("The Dark Tower III", "The Waste Lands", new("The Dark Tower", 3), out decimal? number).Should().BeTrue();
            _ = number.Should().Be(3);
            _ = LibrarySeries.IsSeriesLabel("Dark Tower III", "The Waste Lands", new("The Dark Tower", null), out _).Should().BeTrue();
            _ = LibrarySeries.IsSeriesLabel("H.I.V.E.", "escape velocity", new("H.i.v.e", 3), out number).Should().BeTrue();
            _ = number.Should().BeNull();
            _ = LibrarySeries.IsSeriesLabel("Rocky II", "The Waste Lands", new("The Dark Tower", 3), out _).Should().BeFalse();
            _ = LibrarySeries.IsSeriesLabel("The Dark Tower III", null, new("The Dark Tower", 3), out _).Should().BeFalse();
            // Book 7 is titled for the series; book 1 often is.
            _ = LibrarySeries.IsSeriesLabel("The Dark Tower", null, new("The Dark Tower", 7), out _).Should().BeFalse();
            _ = LibrarySeries.IsSeriesLabel("H.I.V.E.", "the Higher Institute of Villainous Education", new("H.I.V.E.", 1), out _).Should().BeFalse();
            _ = LibrarySeries.IsSeriesLabel("Artemis Fowl", "The Arctic Incident", new("Artemis Fowl", null), out _).Should().BeFalse();
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
