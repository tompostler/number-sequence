using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class ChiroRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentNameTemplate",
                table: "PdfTemplates");

            migrationBuilder.DropColumn(
                name: "SubjectTemplate",
                table: "PdfTemplates");

            migrationBuilder.CreateTable(
                name: "ChiroRecords",
                columns: table => new
                {
                    RowId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DataEnteredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    InputJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiroRecords", x => x.RowId);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ChiroRecords (RowId, Source, DataEnteredAt, RecordedAt, ProcessedAt)
                SELECT RowId, SpreadsheetId, ISNULL(RowCreatedAt, ProcessedAt), ProcessedAt, ProcessedAt
                FROM PdfTemplateSpreadsheetRows
            ");

            migrationBuilder.DropTable(
                name: "PdfTemplateSpreadsheetRows");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiroRecords");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentNameTemplate",
                table: "PdfTemplates",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubjectTemplate",
                table: "PdfTemplates",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PdfTemplateSpreadsheetRows",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    RowCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SpreadsheetId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdfTemplateSpreadsheetRows", x => x.DocumentId);
                });
        }
    }
}
