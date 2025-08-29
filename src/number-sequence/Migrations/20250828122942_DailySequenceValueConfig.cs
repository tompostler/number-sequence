using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class DailySequenceValueConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OriginalValue",
                table: "DailySequenceValues",
                type: "decimal(27,5)",
                precision: 27,
                scale: 5,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DailySequenceValueConfigs",
                columns: table => new
                {
                    Account = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    NegativeDeltaMax = table.Column<decimal>(type: "decimal(27,5)", precision: 27, scale: 5, nullable: true),
                    NegativeDeltaMin = table.Column<decimal>(type: "decimal(27,5)", precision: 27, scale: 5, nullable: true),
                    PositiveDeltaMax = table.Column<decimal>(type: "decimal(27,5)", precision: 27, scale: 5, nullable: true),
                    PositiveDeltaMin = table.Column<decimal>(type: "decimal(27,5)", precision: 27, scale: 5, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySequenceValueConfigs", x => new { x.Account, x.Category });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailySequenceValueConfigs");

            migrationBuilder.DropColumn(
                name: "OriginalValue",
                table: "DailySequenceValues");
        }
    }
}
