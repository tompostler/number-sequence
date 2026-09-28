using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGoogleSheetIngestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedSubmitterEmails",
                table: "PdfTemplates");

            migrationBuilder.DropColumn(
                name: "SpreadsheetId",
                table: "PdfTemplates");

            migrationBuilder.DropColumn(
                name: "SpreadsheetRange",
                table: "PdfTemplates");

            // Rows the google sheet ingestion recorded for disallowed submitters, only so it wouldn't re-read them.
            // Rows with no InputJson that were processed are pre-ChiroRecord history and stay.
            migrationBuilder.Sql(@"
                DELETE FROM ChiroRecords WHERE InputJson IS NULL AND ProcessedAt IS NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowedSubmitterEmails",
                table: "PdfTemplates",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpreadsheetId",
                table: "PdfTemplates",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpreadsheetRange",
                table: "PdfTemplates",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);
        }
    }
}
