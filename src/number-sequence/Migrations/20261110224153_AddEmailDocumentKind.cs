using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailDocumentKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "EmailDocuments",
                type: "NVARCHAR(8)",
                nullable: false,
                defaultValue: "Unknown");

            // Every ledger instance id there has ever been contains invoice or statement: {FriendlyId}_invoice and
            // _statement now, invoice-postler and statement-postler before. Everything else is chiro, whose ids carry
            // the chiro-* template id.
            migrationBuilder.Sql(@"
                UPDATE dbo.EmailDocuments
                SET Kind = CASE WHEN Id LIKE '%invoice%' OR Id LIKE '%statement%' THEN 'Ledger' ELSE 'Chiro' END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                table: "EmailDocuments");
        }
    }
}
