using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class PdfStatusCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command command = new("pdf-status", "Get the status of the pdf background services.");
            command.SetHandler(HandleAsync, stampOption, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            PdfStatus pdfStatus = await client.PdfStatus.GetAsync();

            Console.WriteLine();

            Console.WriteLine(nameof(PdfStatus.TemplateSpreadsheetRows));
            Output.WriteTable(
                pdfStatus.TemplateSpreadsheetRows,
                nameof(PdfStatus.TemplateSpreadsheetRow.RowId),
                nameof(PdfStatus.TemplateSpreadsheetRow.DocumentId),
                nameof(PdfStatus.TemplateSpreadsheetRow.CreatedDate));

            Console.WriteLine(nameof(PdfStatus.Documents));
            Output.WriteTable(
                pdfStatus.Documents,
                nameof(PdfStatus.Document.Id),
                nameof(PdfStatus.Document.CreatedDate),
                nameof(PdfStatus.Document.ProcessedAt),
                nameof(PdfStatus.Document.Delay),
                nameof(PdfStatus.Document.Successful));

            Console.WriteLine(nameof(PdfStatus.EmailDocuments));
            Output.WriteTable(
                pdfStatus.EmailDocuments,
                nameof(PdfStatus.EmailDocument.Id),
                nameof(PdfStatus.EmailDocument.Subject),
                nameof(PdfStatus.EmailDocument.AttachmentName),
                nameof(PdfStatus.EmailDocument.CreatedDate),
                nameof(PdfStatus.EmailDocument.ProcessedAt),
                nameof(PdfStatus.EmailDocument.Delay));

        }
    }
}
