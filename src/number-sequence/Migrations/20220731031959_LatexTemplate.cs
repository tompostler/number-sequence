using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    public partial class LatexTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AttachmentName",
                table: "EmailLatexDocuments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LatexTemplates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SpreadsheetId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SpreadsheetRange = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    EmailTo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AttachmentNameTemplate = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LatexTemplates", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LatexTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "AttachmentName",
                table: "EmailLatexDocuments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);
        }
    }
}
