using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class RenameInvoicingRoleToLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Accounts SET Roles = REPLACE(Roles, 'Invoicing', 'Ledger') WHERE Roles LIKE '%Invoicing%'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Accounts SET Roles = REPLACE(Roles, 'Ledger', 'Invoicing') WHERE Roles LIKE '%Ledger%'");
        }
    }
}
