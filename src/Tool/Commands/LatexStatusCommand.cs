using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class LatexStatusCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command command = new("latex-status", "Get the status of the latex background services.");
            command.SetHandler(HandleAsync, stampOption, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            LatexStatus latexStatus = await client.LatexStatus.GetAsync();

            Console.WriteLine();

            Console.WriteLine(nameof(LatexStatus.LatexTemplateSpreadsheetRows));
            Output.WriteTable(
                latexStatus.LatexTemplateSpreadsheetRows,
                nameof(LatexStatus.LatexTemplateSpreadsheetRow.RowId),
                nameof(LatexStatus.LatexTemplateSpreadsheetRow.LatexDocumentId),
                nameof(LatexStatus.LatexTemplateSpreadsheetRow.CreatedDate));

            Console.WriteLine(nameof(LatexStatus.LatexDocuments));
            Output.WriteTable(
                latexStatus.LatexDocuments,
                nameof(LatexStatus.LatexDocument.Id),
                nameof(LatexStatus.LatexDocument.CreatedDate),
                nameof(LatexStatus.LatexDocument.ProcessedAt),
                nameof(LatexStatus.LatexDocument.Delay),
                nameof(LatexStatus.LatexDocument.Successful));

            Console.WriteLine(nameof(LatexStatus.EmailDocuments));
            Output.WriteTable(
                latexStatus.EmailDocuments,
                nameof(LatexStatus.EmailDocument.Id),
                nameof(LatexStatus.EmailDocument.Subject),
                nameof(LatexStatus.EmailDocument.AttachmentName),
                nameof(LatexStatus.EmailDocument.CreatedDate),
                nameof(LatexStatus.EmailDocument.ProcessedAt),
                nameof(LatexStatus.EmailDocument.Delay));

        }
    }
}
