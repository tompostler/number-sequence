using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfAccountName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "EmailDocuments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "ChiroRecords",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "ChiroEmailBatches",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            // Backfill. Each step only fills what the step before it made derivable, and leaves a row null rather
            // than guess when more than one account could match. Rows from the spreadsheet era stay null.

            // Ui submissions have always carried the submitter in Source as ui/{account}.
            migrationBuilder.Sql(@"
                UPDATE dbo.ChiroRecords
                SET AccountName = SUBSTRING(Source, 4, 64)
                WHERE AccountName IS NULL AND Source LIKE 'ui/%';
            ");

            // Chiro email documents are keyed by orchestration instance id, which starts with the record's RowId
            // through MakeHumanFriendly: the first nine characters as abc-def-ghi. The prefix is built once per record
            // and grouped, so the update is one equality join rather than a scan of every record per document.
            migrationBuilder.Sql(@"
                UPDATE ed
                SET AccountName = m.AccountName
                FROM dbo.EmailDocuments ed
                JOIN (
                    SELECT p.Prefix, MIN(p.AccountName) AS AccountName
                    FROM (
                        SELECT LOWER(SUBSTRING(RowId, 1, 3) + '-' + SUBSTRING(RowId, 4, 3) + '-' + SUBSTRING(RowId, 7, 3)) + '_' AS Prefix, AccountName
                        FROM dbo.ChiroRecords
                        WHERE AccountName IS NOT NULL AND LEN(RowId) >= 9
                    ) p
                    GROUP BY p.Prefix
                    HAVING COUNT(DISTINCT p.AccountName) = 1
                ) m ON m.Prefix = LEFT(ed.Id, 12)
                WHERE ed.AccountName IS NULL;
            ");

            // Ledger email documents are keyed {FriendlyId}_invoice or _statement, FriendlyId being {Id:0000}-{attempt:00}.
            migrationBuilder.Sql(@"
                UPDATE ed
                SET AccountName = i.AccountName
                FROM dbo.EmailDocuments ed
                JOIN dbo.Invoices i ON i.Id = TRY_CAST(LEFT(ed.Id, NULLIF(CHARINDEX('-', ed.Id), 0) - 1) AS bigint)
                WHERE ed.AccountName IS NULL AND ed.Id LIKE '%[_]invoice';

                UPDATE ed
                SET AccountName = s.AccountName
                FROM dbo.EmailDocuments ed
                JOIN dbo.Statements s ON s.Id = TRY_CAST(LEFT(ed.Id, NULLIF(CHARINDEX('-', ed.Id), 0) - 1) AS bigint)
                WHERE ed.AccountName IS NULL AND ed.Id LIKE '%[_]statement';
            ");

            // A batch row's only link back to its pdf is the attachment name, which has no index; grouped for the same
            // reason as above.
            migrationBuilder.Sql(@"
                UPDATE b
                SET AccountName = m.AccountName
                FROM dbo.ChiroEmailBatches b
                JOIN (
                    SELECT AttachmentName, MIN(AccountName) AS AccountName
                    FROM dbo.EmailDocuments
                    WHERE AccountName IS NOT NULL AND AttachmentName IS NOT NULL
                    GROUP BY AttachmentName
                    HAVING COUNT(DISTINCT AccountName) = 1
                ) m ON m.AttachmentName = b.AttachmentName
                WHERE b.AccountName IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "EmailDocuments");

            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "ChiroRecords");

            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "ChiroEmailBatches");
        }
    }
}
