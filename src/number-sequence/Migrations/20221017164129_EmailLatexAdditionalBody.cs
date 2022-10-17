using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    public partial class EmailLatexAdditionalBody : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalBody",
                table: "EmailLatexDocuments",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalBody",
                table: "EmailLatexDocuments");
        }
    }
}
