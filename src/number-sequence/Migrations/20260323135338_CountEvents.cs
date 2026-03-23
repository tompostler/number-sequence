using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class CountEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "CountEventIds");

            migrationBuilder.AddColumn<bool>(
                name: "OverflowDropsOldestEvents",
                table: "Counts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CountEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.CountEventIds"),
                    Account = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    IncrementAmount = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CountEvents_Counts_Account_CountName",
                        columns: x => new { x.Account, x.CountName },
                        principalTable: "Counts",
                        principalColumns: new[] { "Account", "Name" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountEvents_Account_CountName",
                table: "CountEvents",
                columns: new[] { "Account", "CountName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountEvents");

            migrationBuilder.DropColumn(
                name: "OverflowDropsOldestEvents",
                table: "Counts");

            migrationBuilder.DropSequence(
                name: "CountEventIds");
        }
    }
}
