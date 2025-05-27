﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class InvoicingRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceLine");

            migrationBuilder.DropTable(
                name: "InvoiceLineDefaults");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "InvoiceBusinesses");

            migrationBuilder.DropTable(
                name: "InvoiceCustomers");

            migrationBuilder.DropSequence(
                name: "InvoiceBusinessIds");

            migrationBuilder.DropSequence(
                name: "InvoiceCustomerIds");

            migrationBuilder.DropSequence(
                name: "InvoiceIds");

            migrationBuilder.DropSequence(
                name: "InvoiceLineDefaultIds");

            migrationBuilder.DropSequence(
                name: "InvoiceLineIds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "InvoiceBusinessIds");

            migrationBuilder.CreateSequence(
                name: "InvoiceCustomerIds");

            migrationBuilder.CreateSequence(
                name: "InvoiceIds");

            migrationBuilder.CreateSequence(
                name: "InvoiceLineDefaultIds");

            migrationBuilder.CreateSequence(
                name: "InvoiceLineIds");

            migrationBuilder.CreateTable(
                name: "InvoiceBusinesses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.InvoiceBusinessIds"),
                    AccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PayableName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceBusinesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceCustomers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.InvoiceCustomerIds"),
                    AccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLineDefaults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.InvoiceLineDefaultIds"),
                    AccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLineDefaults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.InvoiceIds"),
                    BusinessId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    PaidDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PaidDetails = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ProccessAttempt = table.Column<long>(type: "bigint", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReadyForProcessing = table.Column<bool>(type: "bit", nullable: false),
                    ReprocessRegularly = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_InvoiceBusinesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "InvoiceBusinesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_InvoiceCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "InvoiceCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.InvoiceLineIds"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLine_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_InvoiceId",
                table: "InvoiceLine",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BusinessId",
                table: "Invoices",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");
        }
    }
}
