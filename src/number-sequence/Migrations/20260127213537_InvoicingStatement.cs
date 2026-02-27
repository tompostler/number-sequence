using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class InvoicingStatement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "StatementIds");

            migrationBuilder.AddColumn<long>(
                name: "StatementId",
                table: "Invoices",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Statements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.StatementIds"),
                    AccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    InvoiceStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    InvoiceEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BusinessId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ReadyForProcessing = table.Column<bool>(type: "bit", nullable: false),
                    ProccessAttempt = table.Column<long>(type: "bigint", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Statements_InvoiceBusinesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "InvoiceBusinesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Statements_InvoiceCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "InvoiceCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_StatementId",
                table: "Invoices",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_BusinessId",
                table: "Statements",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_CustomerId",
                table: "Statements",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Statements_StatementId",
                table: "Invoices",
                column: "StatementId",
                principalTable: "Statements",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Statements_StatementId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "Statements");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_StatementId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "StatementId",
                table: "Invoices");

            migrationBuilder.DropSequence(
                name: "StatementIds");
        }
    }
}
