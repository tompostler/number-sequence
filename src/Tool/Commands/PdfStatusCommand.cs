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
            Option<int> takeAmountOption = new("--take")
            {
                Description = "How many records of each type to return, within the days of lookback ordered by most recent first.",
                DefaultValueFactory = _ => 20,
            };
            Option<int> daysLookbackOption = new("--days-lookback")
            {
                Description = "How many days of lookback.",
                DefaultValueFactory = _ => 30,
            };
            Option<FileInfo> chartOption = new("--chart")
            {
                Description = "If specified, writes a PNG stacked bar chart of chiro forms per day to this file instead of printing the tables.",
            };
            Option<ChiroChartGrouping> chartGroupByOption = new("--chart-group-by")
            {
                Description = "What each day's bar in --chart is segmented by.",
                DefaultValueFactory = _ => ChiroChartGrouping.Clinic,
            };
            Command command = new("pdf-status", "Get the status of the pdf background services. Defaults to local time.")
            {
                stampOption,
                verbosityOption,
                takeAmountOption,
                daysLookbackOption,
                chartOption,
                chartGroupByOption,
            };
            command.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    int takeAmount = parseResult.GetRequiredValue(takeAmountOption);
                    int daysLookback = parseResult.GetRequiredValue(daysLookbackOption);
                    FileInfo chart = parseResult.GetValue(chartOption);
                    ChiroChartGrouping chartGroupBy = parseResult.GetRequiredValue(chartGroupByOption);
                    return HandleAsync(stamp, takeAmount, daysLookback, chart, chartGroupBy, verbosity);
                });
            return command;
        }

        private static async Task HandleAsync(Stamp stamp, int takeAmount, int daysLookback, FileInfo chart, ChiroChartGrouping chartGroupBy, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            if (chart != null)
            {
                byte[] bytes = await client.PdfStatus.GetChartAsync(daysLookback, TimeZoneInfo.Local.BaseUtcOffset.TotalHours, groupBy: chartGroupBy);
                if (bytes == null)
                {
                    Console.WriteLine("No chiro records to chart.");
                    return;
                }

                chart.Delete();
                File.WriteAllBytes(chart.FullName, bytes);
                Console.WriteLine($"Wrote {chart.FullName}");
                return;
            }

            PdfStatus pdfStatus = await client.PdfStatus.GetAsync(TimeZoneInfo.Local.BaseUtcOffset.TotalHours, takeAmount, daysLookback);

            Console.WriteLine();

            Console.WriteLine($"{nameof(PdfStatus.ChiroRecords)} ({pdfStatus.ChiroRecords.Count}):");
            Output.WriteTable(
                pdfStatus.ChiroRecords,
                nameof(PdfStatus.ChiroRecord.Id),
                nameof(PdfStatus.ChiroRecord.AccountName),
                // DataEnteredAt and DictationParsedAt are RecordedAt less EntryTime and ReviewTime, and too wide to also show.
                nameof(PdfStatus.ChiroRecord.RecordedAt),
                nameof(PdfStatus.ChiroRecord.ProcessedAt),
                nameof(PdfStatus.ChiroRecord.EntryTime),
                nameof(PdfStatus.ChiroRecord.ReviewTime),
                nameof(PdfStatus.ChiroRecord.Delay),
                nameof(PdfStatus.ChiroRecord.ProcessAttempt));

            static void writeEmailDocuments(string name, List<PdfStatus.EmailDocument> documents)
            {
                Console.WriteLine($"{name} ({documents.Count}):");
                Output.WriteTable(
                    documents,
                    nameof(PdfStatus.EmailDocument.Id),
                    nameof(PdfStatus.EmailDocument.AccountName),
                    nameof(PdfStatus.EmailDocument.Subject),
                    nameof(PdfStatus.EmailDocument.AttachmentName),
                    nameof(PdfStatus.EmailDocument.FileLength),
                    nameof(PdfStatus.EmailDocument.CreatedDate),
                    nameof(PdfStatus.EmailDocument.ProcessedAt),
                    nameof(PdfStatus.EmailDocument.Delay));
            }

            writeEmailDocuments(nameof(PdfStatus.ChiroEmailDocuments), pdfStatus.ChiroEmailDocuments);
            writeEmailDocuments(nameof(PdfStatus.LedgerEmailDocuments), pdfStatus.LedgerEmailDocuments);
            if (pdfStatus.UnclassifiedEmailDocuments.Count > 0)
            {
                writeEmailDocuments(nameof(PdfStatus.UnclassifiedEmailDocuments), pdfStatus.UnclassifiedEmailDocuments);
            }

            Console.WriteLine($"{nameof(PdfStatus.ChiroBatches)} ({pdfStatus.ChiroBatches.Count}):");
            Output.WriteTable(
                pdfStatus.ChiroBatches,
                nameof(PdfStatus.ChiroBatch.Id),
                nameof(PdfStatus.ChiroBatch.AccountName),
                nameof(PdfStatus.ChiroBatch.Recipient),
                nameof(PdfStatus.ChiroBatch.AttachmentName),
                nameof(PdfStatus.ChiroBatch.FileLength),
                nameof(PdfStatus.ChiroBatch.CreatedDate),
                nameof(PdfStatus.ChiroBatch.ProcessedAt),
                nameof(PdfStatus.ChiroBatch.Delay));

            Console.WriteLine($"{nameof(PdfStatus.ChiroRecordsPending)} ({pdfStatus.ChiroRecordsPending.Count}):");
            Output.WriteTable(
                pdfStatus.ChiroRecordsPending,
                nameof(PdfStatus.ChiroRecord.Id),
                nameof(PdfStatus.ChiroRecord.AccountName),
                nameof(PdfStatus.ChiroRecord.DataEnteredAt),
                nameof(PdfStatus.ChiroRecord.RecordedAt),
                nameof(PdfStatus.ChiroRecord.Delay),
                nameof(PdfStatus.ChiroRecord.ProcessAttempt));

            Console.WriteLine($"{nameof(PdfStatus.ChiroBatchPendingCounts)} ({pdfStatus.ChiroBatchPendingCounts.Count}):");
            Output.WriteTable(
                pdfStatus.ChiroBatchPendingCounts,
                nameof(PdfStatus.ChiroBatchPendingCount.Recipient),
                nameof(PdfStatus.ChiroBatchPendingCount.Count),
                nameof(PdfStatus.ChiroBatchPendingCount.TotalFileLength));
        }
    }
}
