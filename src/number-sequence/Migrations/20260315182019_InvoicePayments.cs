using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class InvoicePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "InvoicePaymentIds");

            migrationBuilder.CreateTable(
                name: "InvoicePayments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.InvoicePaymentIds"),
                    AccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoicePayments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_InvoiceId",
                table: "InvoicePayments",
                column: "InvoiceId");

            // Migrate existing fully-paid invoices: create one InvoicePayment per invoice where
            // PaidDate IS NOT NULL, using the invoice line total as the payment amount and
            // PaidDetails as the payment details.
            migrationBuilder.Sql(@"
                INSERT INTO InvoicePayments (AccountName, InvoiceId, Amount, PaymentDate, Details)
                SELECT
                    i.AccountName,
                    i.Id,
                    COALESCE((SELECT SUM(il.Quantity * il.Price) FROM InvoiceLines il WHERE il.InvoiceId = i.Id), 0),
                    i.PaidDate,
                    i.PaidDetails
                FROM Invoices i
                WHERE i.PaidDate IS NOT NULL
            ");

            migrationBuilder.DropColumn(
                name: "PaidDetails",
                table: "Invoices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoicePayments");

            migrationBuilder.DropSequence(
                name: "InvoicePaymentIds");

            migrationBuilder.AddColumn<string>(
                name: "PaidDetails",
                table: "Invoices",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}
