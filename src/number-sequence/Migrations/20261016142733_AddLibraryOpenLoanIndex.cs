using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryOpenLoanIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LibraryLoans_CopyId_Open",
                table: "LibraryLoans",
                column: "CopyId",
                unique: true,
                filter: "[ReturnedDate] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LibraryLoans_CopyId_Open",
                table: "LibraryLoans");
        }
    }
}
