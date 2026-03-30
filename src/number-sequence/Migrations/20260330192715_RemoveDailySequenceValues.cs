using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDailySequenceValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailySequenceValueConfigs");

            migrationBuilder.DropTable(
                name: "DailySequenceValues");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailySequenceValueConfigs",
                columns: table => new
                {
                    Account = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    NegativeDeltaMax = table.Column<decimal>(type: "decimal(27,5)", precision: 27, scale: 5, nullable: true),
                    NegativeDeltaMin = table.Column<decimal>(type: "decimal(27,5)", precision: 27, scale: 5, nullable: true),
                    PositiveDeltaMax = table.Column<decimal>(type: "decimal(27,5)", precision: 27, scale: 5, nullable: true),
                    PositiveDeltaMin = table.Column<decimal>(type: "decimal(27,5)", precision: 27, scale: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySequenceValueConfigs", x => new { x.Account, x.Category });
                });

            migrationBuilder.CreateTable(
                name: "DailySequenceValues",
                columns: table => new
                {
                    Account = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    OriginalValue = table.Column<decimal>(type: "decimal(27,5)", precision: 27, scale: 5, nullable: true),
                    Value = table.Column<decimal>(type: "decimal(27,5)", precision: 27, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySequenceValues", x => new { x.Account, x.Category, x.EventDate });
                });
        }
    }
}
