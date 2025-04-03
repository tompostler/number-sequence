using Cronos;
using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Services.Background.GoogleSheetPdfGeneration
{
    public sealed class ChiroCanineGoogleSheetPdfGenerationBackgroundService : SqlSynchronizedBackgroundService
    {
        private readonly GoogleSheetDataAccess googleSheetDataAccess;

        public ChiroCanineGoogleSheetPdfGenerationBackgroundService(
            GoogleSheetDataAccess googleSheetDataAccess,
            IOptions<Options.Google> googleOptions,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ChiroCanineGoogleSheetPdfGenerationBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        {
            this.googleSheetDataAccess = googleSheetDataAccess;
        }

        protected override List<CronExpression> Crons => new()
        {
            // 45 seconds into the minute, every 15 minutes, 9AM through 10PM, Monday through Friday
            CronExpression.Parse("45 */15 9-22 * * MON-FRI", CronFormat.IncludeSeconds),
            // 45 seconds into the minute, every hour, 12AM through 9AM and 10PM through 12AM, Monday through Friday
            CronExpression.Parse("45 0 0-9,23 * * MON-FRI", CronFormat.IncludeSeconds),
            // 45 seconds into the minute, every hour, Saturday through Sunday
            CronExpression.Parse("45 0 * * * SAT-SUN", CronFormat.IncludeSeconds),
        };

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Get the template information.
            PdfTemplate template = await nsContext.PdfTemplates.FirstOrDefaultAsync(x => x.Id == NsStorage.C.PT.ChiroCanine, cancellationToken);
            if (template == default)
            {
                this.logger.LogInformation("No template defined.");
                return;
            }

            // Skip the number of rows that we already know about.
            int numberOfKnownRows = await nsContext.PdfTemplateSpreadsheetRows
                .CountAsync(x => x.SpreadsheetId == template.SpreadsheetId, cancellationToken);
            string spreadsheetRange = template.SpreadsheetRange.Replace("1", Math.Max(1, numberOfKnownRows).ToString());
            this.logger.LogInformation($"Skipping {numberOfKnownRows} known rows and querying range {spreadsheetRange}");

            // Get the data from the spreadsheet. The first row is the headers (or a previously processed row).
            IList<IList<object>> data = await this.googleSheetDataAccess.GetAsync(template.SpreadsheetId, spreadsheetRange, cancellationToken);
            IList<object> headers = data[0];
            data = data.Skip(1).ToList();

            // Only on reset or initial deployment, no data.
            if (!data.Any())
            {
                this.logger.LogInformation("No rows of data.");
                return;
            }

            // Check each row of data to see if it's already been processed.
            // Only process one additional row at a time.
            string[] row = default;
            PdfTemplateSpreadsheetRow pdfTemplateRow = default;
            for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
            {
                row = data[rowIndex].Select(x => x as string).ToArray();
                string id = string.Join('|', row).ComputeSHA256();
                pdfTemplateRow = await nsContext.PdfTemplateSpreadsheetRows
                    .SingleOrDefaultAsync(x => x.SpreadsheetId == template.SpreadsheetId && x.RowId == id, cancellationToken);

                if (pdfTemplateRow != default)
                {
                    this.logger.LogInformation($"Data row {id} ({rowIndex + numberOfKnownRows + 1}) was created at {pdfTemplateRow.RowCreatedAt:u} and recorded at {pdfTemplateRow.ProcessedAt:u}");
                    continue;
                }
                else
                {
                    this.logger.LogInformation($"Data row {id} ({rowIndex + numberOfKnownRows + 1}) is new. Setting up for processing.");

                    TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
                    OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                        typeof(DurableTaskImpl.Orchestrators.ChiroCanineGenerationOrchestrator),
                        instanceId: $"{id.MakeHumanFriendly()}_{template.Id}",
                        rowIndex + numberOfKnownRows);
                    this.logger.LogInformation($"Created orchestration {instance.InstanceId} to generate the pdf.");
                    break;
                }
            }
        }
    }
}
