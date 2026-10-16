using FluentAssertions;
using number_sequence.Utilities;

namespace number_sequence.UnitTests.UtilityTests
{
    [TestClass]
    public sealed class LibraryBarcodeTests
    {
        [TestMethod]
        [DataRow("9780306406157", "9780306406157", LibraryBarcodeKind.Isbn, DisplayName = "ISBN-13 as scanned")]
        [DataRow("978-0-306-40615-7", "9780306406157", LibraryBarcodeKind.Isbn, DisplayName = "ISBN-13 with dashes")]
        [DataRow("0-306-40615-2", "9780306406157", LibraryBarcodeKind.Isbn, DisplayName = "ISBN-10 becomes the same ISBN-13")]
        [DataRow("0 8044 2957 x", "9780804429573", LibraryBarcodeKind.Isbn, DisplayName = "ISBN-10 with lowercase X check")]
        [DataRow("036000291452", "0036000291452", LibraryBarcodeKind.Gtin, DisplayName = "UPC-A gets a leading zero")]
        [DataRow("0036000291452", "0036000291452", LibraryBarcodeKind.Gtin, DisplayName = "UPC-A read as EAN-13 is the same")]
        [DataRow("4006381333931", "4006381333931", LibraryBarcodeKind.Gtin, DisplayName = "EAN-13")]
        [DataRow("96385074", "96385074", LibraryBarcodeKind.Other, DisplayName = "8 digits are left alone")]
        [DataRow(" box-12 ", "BOX12", LibraryBarcodeKind.Other, DisplayName = "Own label")]
        public void TryParseNormalizes(string input, string expected, LibraryBarcodeKind expectedKind)
        {
            // Act
            bool parsed = LibraryBarcode.TryParse(input, out LibraryBarcode barcode, out string error);

            // Assert
            _ = parsed.Should().BeTrue(error);
            _ = barcode.Value.Should().Be(expected);
            _ = barcode.Kind.Should().Be(expectedKind);
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Null")]
        [DataRow(" - ", DisplayName = "Only separators")]
        [DataRow("9780306406158", DisplayName = "ISBN-13 with a wrong check digit")]
        [DataRow("0306406153", DisplayName = "ISBN-10 with a wrong check digit")]
        [DataRow("036000291453", DisplayName = "UPC-A with a wrong check digit")]
        [DataRow("123456789012345678901234567890123", DisplayName = "Longer than the column")]
        public void TryParseRejects(string input)
        {
            // Act
            bool parsed = LibraryBarcode.TryParse(input, out _, out string error);

            // Assert
            _ = parsed.Should().BeFalse();
            _ = error.Should().NotBeNullOrEmpty();
        }
    }
}
