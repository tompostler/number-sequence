using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class LatexDocumentIdToPdfRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LatexDocumentId",
                table: "PdfTemplateSpreadsheetRows",
                newName: "DocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "PdfTemplateSpreadsheetRows",
                newName: "LatexDocumentId");
        }
    }
}
