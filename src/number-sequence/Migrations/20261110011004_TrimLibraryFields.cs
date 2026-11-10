using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class TrimLibraryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LibraryCopies_LibraryId_Isbn",
                table: "LibraryCopies");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "LibraryItems");

            migrationBuilder.DropColumn(
                name: "AcquiredFrom",
                table: "LibraryCopies");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "LibraryCopies");

            migrationBuilder.DropColumn(
                name: "Isbn",
                table: "LibraryCopies");

            migrationBuilder.DropColumn(
                name: "PricePaid",
                table: "LibraryCopies");

            migrationBuilder.DropColumn(
                name: "PublishedYear",
                table: "LibraryCopies");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "LibraryCopies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "LibraryItems",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcquiredFrom",
                table: "LibraryCopies",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "LibraryCopies",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isbn",
                table: "LibraryCopies",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePaid",
                table: "LibraryCopies",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublishedYear",
                table: "LibraryCopies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "LibraryCopies",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryCopies_LibraryId_Isbn",
                table: "LibraryCopies",
                columns: new[] { "LibraryId", "Isbn" });
        }
    }
}
