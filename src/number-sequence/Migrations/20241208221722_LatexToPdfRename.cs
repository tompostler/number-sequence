using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class LatexToPdfRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "LatexDocuments",
                newName: "PdfDocuments");

            migrationBuilder.RenameTable(
                name: "LatexTemplates",
                newName: "PdfTemplates");

            migrationBuilder.RenameTable(
                name: "LatexTemplateSpreadsheetRows",
                newName: "PdfTemplateSpreadsheetRows");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "PdfDocuments",
                newName: "LatexDocuments");

            migrationBuilder.RenameTable(
                name: "PdfTemplates",
                newName: "LatexTemplates");

            migrationBuilder.RenameTable(
                name: "PdfTemplateSpreadsheetRows",
                newName: "LatexTemplateSpreadsheetRows");
        }
    }
}
