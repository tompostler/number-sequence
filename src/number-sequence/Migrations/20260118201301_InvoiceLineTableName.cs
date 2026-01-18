using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceLineTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Invoices_InvoiceId",
                table: "InvoiceLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InvoiceLine",
                table: "InvoiceLine");

            migrationBuilder.RenameTable(
                name: "InvoiceLine",
                newName: "InvoiceLines");

            migrationBuilder.RenameIndex(
                name: "IX_InvoiceLine_InvoiceId",
                table: "InvoiceLines",
                newName: "IX_InvoiceLines_InvoiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InvoiceLines",
                table: "InvoiceLines",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLines_Invoices_InvoiceId",
                table: "InvoiceLines",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLines_Invoices_InvoiceId",
                table: "InvoiceLines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InvoiceLines",
                table: "InvoiceLines");

            migrationBuilder.RenameTable(
                name: "InvoiceLines",
                newName: "InvoiceLine");

            migrationBuilder.RenameIndex(
                name: "IX_InvoiceLines_InvoiceId",
                table: "InvoiceLine",
                newName: "IX_InvoiceLine_InvoiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InvoiceLine",
                table: "InvoiceLine",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Invoices_InvoiceId",
                table: "InvoiceLine",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id");
        }
    }
}
