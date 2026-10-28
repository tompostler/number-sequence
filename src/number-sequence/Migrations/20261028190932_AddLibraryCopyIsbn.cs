using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryCopyIsbn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Isbn",
                table: "LibraryCopies",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryCopies_LibraryId_Isbn",
                table: "LibraryCopies",
                columns: new[] { "LibraryId", "Isbn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LibraryCopies_LibraryId_Isbn",
                table: "LibraryCopies");

            migrationBuilder.DropColumn(
                name: "Isbn",
                table: "LibraryCopies");
        }
    }
}
