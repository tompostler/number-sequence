using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class DaysSince : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "DaysSinceEventIds");

            migrationBuilder.CreateTable(
                name: "DaysSinces",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LastOccurrence = table.Column<DateOnly>(type: "date", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ValueLine1 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ValueLine2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ValueLine3 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ValueLine4 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaysSinces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DaysSinceEvent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.DaysSinceEventIds"),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    DaysSinceId = table.Column<string>(type: "nvarchar(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaysSinceEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DaysSinceEvent_DaysSinces_DaysSinceId",
                        column: x => x.DaysSinceId,
                        principalTable: "DaysSinces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DaysSinceEvent_DaysSinceId",
                table: "DaysSinceEvent",
                column: "DaysSinceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DaysSinceEvent");

            migrationBuilder.DropTable(
                name: "DaysSinces");

            migrationBuilder.DropSequence(
                name: "DaysSinceEventIds");
        }
    }
}
