using FluentAssertions;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace number_sequence.UnitTests.UtilityTests
{
    [TestClass]
    public sealed class LibrarySortTests
    {
        [TestMethod]
        [DataRow("Stephen King", "King Stephen")]
        [DataRow("Robert Jordan; Brandon Sanderson", "Jordan Robert")]
        [DataRow("J.R.R. Tolkien", "Tolkien J.R.R.")]
        [DataRow("Ursula K. Le Guin", "Le Guin Ursula K.")]
        [DataRow("Ludwig van Beethoven", "van Beethoven Ludwig")]
        [DataRow("Van Morrison", "Morrison Van")]
        [DataRow("Martin Luther King, Jr.", "King Martin Luther")]
        [DataRow("Kurt Vonnegut Jr", "Vonnegut Kurt")]
        [DataRow("Henry Louis Gates III", "Gates Henry Louis")]
        [DataRow("Tolkien, J.R.R.", "Tolkien J.R.R.")]
        [DataRow("Plato", "Plato")]
        [DataRow("  ;  ", "")]
        [DataRow(null, "")]
        public void CreatorKeyIsLastNameFirst(string creators, string expected)
        {
            // Act
            string key = LibrarySort.CreatorKey(creators);

            // Assert
            _ = key.Should().Be(expected);
        }

        [TestMethod]
        [DataRow("The Waste Lands", "Waste Lands")]
        [DataRow("A Game of Thrones", "Game of Thrones")]
        [DataRow("An Echo of Things to Come", "Echo of Things to Come")]
        [DataRow("the dark tower", "dark tower")]
        [DataRow("Theodore Rex", "Theodore Rex")]
        [DataRow("Anathem", "Anathem")]
        [DataRow("The", "The")]
        [DataRow(null, "")]
        public void TitleKeyDropsLeadingArticle(string title, string expected)
        {
            // Act
            string key = LibrarySort.TitleKey(title);

            // Assert
            _ = key.Should().Be(expected);
        }

        [TestMethod]
        public void ShelfOrderGroupsSeriesAmongAnAuthorsTitles()
        {
            // Arrange
            List<LibraryItem> items =
            [
                new() { Id = 1, Title = "Firestarter", Creators = "Stephen King" },
                new() { Id = 2, Title = "The Waste Lands", Creators = "Stephen King", Series = "The Dark Tower", SeriesNumber = 3 },
                new() { Id = 3, Title = "Cujo", Creators = "Stephen King" },
                new() { Id = 4, Title = "The Gunslinger", Creators = "Stephen King", Series = "The Dark Tower", SeriesNumber = 1 },
                new() { Id = 5, Title = "The Wind Through the Keyhole", Creators = "Stephen King", Series = "The Dark Tower" },
                new() { Id = 6, Title = "Anonymous Pamphlet" },
                new() { Id = 7, Title = "The Eye of the World", Creators = "Robert Jordan", Series = "The Wheel of Time", SeriesNumber = 1 },
            ];

            // Act
            List<long> order = LibrarySort.Order(items, LibraryItemSort.Shelf).Select(x => x.Id).ToList();

            // Assert
            _ = order.Should().Equal(7, 3, 4, 2, 5, 1, 6);
        }
    }
}
