using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace number_sequence.Migrations
{
    /// <inheritdoc />
    public partial class ChiroBatchEmailCc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "ChiroEmailBatchIds",
                startValue: 500L);

            migrationBuilder.AlterColumn<string>(
                name: "ClinicAbbreviation",
                table: "ChiroEmailBatches",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ChiroEmailBatches",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "CAST(NEXT VALUE FOR dbo.ChiroEmailBatchIds AS nvarchar(20))",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<string>(
                name: "CcEmail",
                table: "ChiroEmailBatches",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CcEmail",
                table: "ChiroEmailBatches");

            migrationBuilder.DropSequence(
                name: "ChiroEmailBatchIds");

            migrationBuilder.AlterColumn<string>(
                name: "ClinicAbbreviation",
                table: "ChiroEmailBatches",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "ChiroEmailBatches",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldDefaultValueSql: "CAST(NEXT VALUE FOR dbo.ChiroEmailBatchIds AS nvarchar(20))");
        }
    }
}
