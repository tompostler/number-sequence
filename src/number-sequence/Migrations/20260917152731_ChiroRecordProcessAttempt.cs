using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class ChiroRecordProcessAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcessAttempt",
                table: "ChiroRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessAttempt",
                table: "ChiroRecords");
        }
    }
}
