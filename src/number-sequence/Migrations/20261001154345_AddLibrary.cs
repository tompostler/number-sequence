using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class AddLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "LibraryCopyEventIds");

            migrationBuilder.CreateSequence(
                name: "LibraryCopyIds");

            migrationBuilder.CreateSequence(
                name: "LibraryIds");

            migrationBuilder.CreateSequence(
                name: "LibraryItemIds");

            migrationBuilder.CreateSequence(
                name: "LibraryLoanIds");

            migrationBuilder.CreateSequence(
                name: "LibraryLocationIds");

            migrationBuilder.CreateSequence(
                name: "LibraryScanEntryIds");

            migrationBuilder.CreateSequence(
                name: "LibraryScanSessionIds");

            migrationBuilder.AlterColumn<string>(
                name: "Roles",
                table: "Accounts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.LibraryIds"),
                    AccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LibraryItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.LibraryItemIds"),
                    LibraryId = table.Column<long>(type: "bigint", nullable: false),
                    MediaType = table.Column<string>(type: "NVARCHAR(32)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Subtitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Creators = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Series = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SeriesNumber = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExternalIds = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    HasCover = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryItems_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryLocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.LibraryLocationIds"),
                    LibraryId = table.Column<long>(type: "bigint", nullable: false),
                    ParentLocationId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryLocations_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryLocations_LibraryLocations_ParentLocationId",
                        column: x => x.ParentLocationId,
                        principalTable: "LibraryLocations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LibraryShares",
                columns: table => new
                {
                    LibraryId = table.Column<long>(type: "bigint", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Permission = table.Column<string>(type: "NVARCHAR(8)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryShares", x => new { x.LibraryId, x.AccountName });
                    table.ForeignKey(
                        name: "FK_LibraryShares_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryCopies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.LibraryCopyIds"),
                    LibraryId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Publisher = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PublishedYear = table.Column<int>(type: "int", nullable: true),
                    LocationId = table.Column<long>(type: "bigint", nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    AcquiredDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AcquiredFrom = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PricePaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR(16)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryCopies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryCopies_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LibraryCopies_LibraryItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "LibraryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryCopies_LibraryLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "LibraryLocations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LibraryScanSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.LibraryScanSessionIds"),
                    LibraryId = table.Column<long>(type: "bigint", nullable: false),
                    LocationId = table.Column<long>(type: "bigint", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CompletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryScanSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryScanSessions_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryScanSessions_LibraryLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "LibraryLocations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LibraryCopyEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.LibraryCopyEventIds"),
                    CopyId = table.Column<long>(type: "bigint", nullable: false),
                    EventType = table.Column<string>(type: "NVARCHAR(16)", nullable: false),
                    FromLocationId = table.Column<long>(type: "bigint", nullable: true),
                    ToLocationId = table.Column<long>(type: "bigint", nullable: true),
                    LoanId = table.Column<long>(type: "bigint", nullable: true),
                    AccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryCopyEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryCopyEvents_LibraryCopies_CopyId",
                        column: x => x.CopyId,
                        principalTable: "LibraryCopies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryLoans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.LibraryLoanIds"),
                    CopyId = table.Column<long>(type: "bigint", nullable: false),
                    BorrowerName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BorrowerAccountName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LoanedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReturnedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryLoans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryLoans_LibraryCopies_CopyId",
                        column: x => x.CopyId,
                        principalTable: "LibraryCopies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryScanEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.LibraryScanEntryIds"),
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Resolution = table.Column<string>(type: "NVARCHAR(16)", nullable: false),
                    ResolvedCopyId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryScanEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryScanEntries_LibraryScanSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "LibraryScanSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_AccountName",
                table: "Libraries",
                column: "AccountName");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryCopies_ItemId",
                table: "LibraryCopies",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryCopies_LibraryId_Barcode",
                table: "LibraryCopies",
                columns: new[] { "LibraryId", "Barcode" });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryCopies_LocationId",
                table: "LibraryCopies",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryCopyEvents_CopyId",
                table: "LibraryCopyEvents",
                column: "CopyId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryItems_LibraryId_Title",
                table: "LibraryItems",
                columns: new[] { "LibraryId", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryLoans_CopyId",
                table: "LibraryLoans",
                column: "CopyId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryLocations_LibraryId",
                table: "LibraryLocations",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryLocations_ParentLocationId",
                table: "LibraryLocations",
                column: "ParentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScanEntries_SessionId",
                table: "LibraryScanEntries",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScanSessions_LibraryId",
                table: "LibraryScanSessions",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScanSessions_LocationId",
                table: "LibraryScanSessions",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryShares_AccountName",
                table: "LibraryShares",
                column: "AccountName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryCopyEvents");

            migrationBuilder.DropTable(
                name: "LibraryLoans");

            migrationBuilder.DropTable(
                name: "LibraryScanEntries");

            migrationBuilder.DropTable(
                name: "LibraryShares");

            migrationBuilder.DropTable(
                name: "LibraryCopies");

            migrationBuilder.DropTable(
                name: "LibraryScanSessions");

            migrationBuilder.DropTable(
                name: "LibraryItems");

            migrationBuilder.DropTable(
                name: "LibraryLocations");

            migrationBuilder.DropTable(
                name: "Libraries");

            migrationBuilder.DropSequence(
                name: "LibraryCopyEventIds");

            migrationBuilder.DropSequence(
                name: "LibraryCopyIds");

            migrationBuilder.DropSequence(
                name: "LibraryIds");

            migrationBuilder.DropSequence(
                name: "LibraryItemIds");

            migrationBuilder.DropSequence(
                name: "LibraryLoanIds");

            migrationBuilder.DropSequence(
                name: "LibraryLocationIds");

            migrationBuilder.DropSequence(
                name: "LibraryScanEntryIds");

            migrationBuilder.DropSequence(
                name: "LibraryScanSessionIds");

            migrationBuilder.AlterColumn<string>(
                name: "Roles",
                table: "Accounts",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}
