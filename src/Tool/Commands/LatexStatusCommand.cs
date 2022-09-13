using System;
using System.CommandLine;
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

            Console.WriteLine(nameof(LatexStatus.EmailLatexDocuments));
            Output.WriteTable(
                latexStatus.EmailLatexDocuments,
                nameof(LatexStatus.EmailLatexDocument.Id),
                nameof(LatexStatus.EmailLatexDocument.Subject),
                nameof(LatexStatus.EmailLatexDocument.AttachmentName),
                nameof(LatexStatus.EmailLatexDocument.CreatedDate),
                nameof(LatexStatus.EmailLatexDocument.ProcessedAt),
                nameof(LatexStatus.EmailLatexDocument.Delay));

        }
    }
}
