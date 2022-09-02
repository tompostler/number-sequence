using System;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class LatexStatusCommand
    {
        public static Command Create(Option<Verbosity> verbosityOption)
        {
            Command command = new("latex-status", "Get the status of the latex background services.");
            command.SetHandler(HandleAsync, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            LatexStatus latexStatus = await client.LatexStatus.GetAsync();

            StringBuilder sb = new();
            _ = sb.AppendLine();

            _ = sb.AppendLine(nameof(latexStatus.LatexTemplateSpreadsheetRows));
            int col1Len = Math.Max(
                nameof(LatexStatus.LatexTemplateSpreadsheetRow.RowId).Length,
                latexStatus.LatexTemplateSpreadsheetRows.Max(x => x.RowId?.Length ?? 0));
            int col2Len = Math.Max(
                nameof(LatexStatus.LatexTemplateSpreadsheetRow.LatexDocumentId).Length,
                latexStatus.LatexTemplateSpreadsheetRows.Max(x => x.LatexDocumentId?.Length ?? 0));
            int col3Len = Math.Max(
                nameof(LatexStatus.LatexTemplateSpreadsheetRow.CreatedDate).Length,
                latexStatus.LatexTemplateSpreadsheetRows.Max(x => x.CreatedDate?.Length ?? 0));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.LatexTemplateSpreadsheetRow.RowId).PadRight(col1Len));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.LatexTemplateSpreadsheetRow.LatexDocumentId).PadRight(col2Len));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.LatexTemplateSpreadsheetRow.CreatedDate).PadRight(col3Len));
            _ = sb.AppendLine();
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col1Len));
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col2Len));
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col3Len));
            _ = sb.AppendLine();
            foreach (LatexStatus.LatexTemplateSpreadsheetRow row in latexStatus.LatexTemplateSpreadsheetRows)
            {
                _ = sb.Append(' ');
                _ = sb.Append((row.RowId ?? string.Empty).PadRight(col1Len));
                _ = sb.Append(' ');
                _ = sb.Append((row.LatexDocumentId ?? string.Empty).PadRight(col2Len));
                _ = sb.Append(' ');
                _ = sb.Append((row.CreatedDate ?? string.Empty).PadRight(col3Len));
                _ = sb.AppendLine();
            }
            _ = sb.AppendLine();

            _ = sb.AppendLine(nameof(latexStatus.LatexDocuments));
            col1Len = Math.Max(
                nameof(LatexStatus.LatexDocument.Id).Length,
                latexStatus.LatexDocuments.Max(x => x.Id?.Length ?? 0));
            col2Len = Math.Max(
                nameof(LatexStatus.LatexDocument.CreatedDate).Length,
                latexStatus.LatexDocuments.Max(x => x.CreatedDate?.Length ?? 0));
            col3Len = Math.Max(
                nameof(LatexStatus.LatexDocument.ProcessedAt).Length,
                latexStatus.LatexDocuments.Max(x => x.ProcessedAt?.Length ?? 0));
            int col4Len = Math.Max(
                nameof(LatexStatus.LatexDocument.Successful).Length,
                latexStatus.LatexDocuments.Max(x => x.Successful?.Length ?? 0));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.LatexDocument.Id).PadRight(col1Len));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.LatexDocument.CreatedDate).PadRight(col2Len));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.LatexDocument.ProcessedAt).PadRight(col3Len));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.LatexDocument.Successful).PadRight(col4Len));
            _ = sb.AppendLine();
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col1Len));
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col2Len));
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col3Len));
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col4Len));
            _ = sb.AppendLine();
            foreach (LatexStatus.LatexDocument row in latexStatus.LatexDocuments)
            {
                _ = sb.Append(' ');
                _ = sb.Append((row.Id ?? string.Empty).PadRight(col1Len));
                _ = sb.Append(' ');
                _ = sb.Append((row.CreatedDate ?? string.Empty).PadRight(col2Len));
                _ = sb.Append(' ');
                _ = sb.Append((row.ProcessedAt ?? string.Empty).PadRight(col3Len));
                _ = sb.Append(' ');
                _ = sb.Append((row.Successful ?? string.Empty).PadRight(col4Len));
                _ = sb.AppendLine();
            }
            _ = sb.AppendLine();

            _ = sb.AppendLine(nameof(latexStatus.LatexDocuments));
            col1Len = Math.Max(
                nameof(LatexStatus.EmailLatexDocument.Id).Length,
                latexStatus.EmailLatexDocuments.Max(x => x.Id?.Length ?? 0));
            col2Len = Math.Max(
                nameof(LatexStatus.EmailLatexDocument.Subject).Length,
                latexStatus.EmailLatexDocuments.Max(x => x.Subject?.Length ?? 0));
            col3Len = Math.Max(
                nameof(LatexStatus.EmailLatexDocument.AttachmentName).Length,
                latexStatus.EmailLatexDocuments.Max(x => x.AttachmentName?.Length ?? 0));
            col4Len = Math.Max(
                nameof(LatexStatus.EmailLatexDocument.CreatedDate).Length,
                latexStatus.EmailLatexDocuments.Max(x => x.CreatedDate?.Length ?? 0));
            int col5Len = Math.Max(
                nameof(LatexStatus.EmailLatexDocument.ProcessedAt).Length,
                latexStatus.EmailLatexDocuments.Max(x => x.ProcessedAt?.Length ?? 0));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.EmailLatexDocument.Id).PadRight(col1Len));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.EmailLatexDocument.Subject).PadRight(col2Len));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.EmailLatexDocument.AttachmentName).PadRight(col3Len));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.EmailLatexDocument.CreatedDate).PadRight(col4Len));
            _ = sb.Append(' ');
            _ = sb.Append(nameof(LatexStatus.EmailLatexDocument.ProcessedAt).PadRight(col5Len));
            _ = sb.AppendLine();
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col1Len));
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col2Len));
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col3Len));
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col4Len));
            _ = sb.Append(' ');
            _ = sb.Append(new string('-', col5Len));
            _ = sb.AppendLine();
            foreach (LatexStatus.EmailLatexDocument row in latexStatus.EmailLatexDocuments)
            {
                _ = sb.Append(' ');
                _ = sb.Append((row.Id ?? string.Empty).PadRight(col1Len));
                _ = sb.Append(' ');
                _ = sb.Append((row.Subject ?? string.Empty).PadRight(col2Len));
                _ = sb.Append(' ');
                _ = sb.Append((row.AttachmentName ?? string.Empty).PadRight(col3Len));
                _ = sb.Append(' ');
                _ = sb.Append((row.CreatedDate ?? string.Empty).PadRight(col4Len));
                _ = sb.Append(' ');
                _ = sb.Append((row.ProcessedAt ?? string.Empty).PadRight(col5Len));
                _ = sb.AppendLine();
            }
            _ = sb.AppendLine();

            Console.WriteLine(sb.ToString());
        }
    }
}
