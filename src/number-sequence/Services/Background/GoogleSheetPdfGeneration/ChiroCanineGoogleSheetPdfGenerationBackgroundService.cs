using Cronos;
using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using System.Globalization;
using System.Text.Json;
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
            int numberOfKnownRows = await nsContext.ChiroRecords
                .CountAsync(x => x.Source == template.SpreadsheetId, cancellationToken);
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
            for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
            {
                string[] row = data[rowIndex].Select(x => x as string).ToArray();
                string id = (template.SpreadsheetId + "|" + string.Join('|', row)).ComputeSHA256();
                bool alreadyRecorded = await nsContext.ChiroRecords
                    .AnyAsync(x => x.RowId == id, cancellationToken);

                if (alreadyRecorded)
                {
                    this.logger.LogInformation($"Data row {id} ({rowIndex + numberOfKnownRows + 1}) already recorded.");
                    continue;
                }

                this.logger.LogInformation($"Data row {id} ({rowIndex + numberOfKnownRows + 1}) is new. Setting up for processing.");

                static string safeGet(string[] r, int i) => i < r.Length ? r[i]?.Trim() : string.Empty;

                ChiroInput input = new()
                {
                    RowCreatedAt = new DateTimeOffset(DateTime.ParseExact(row[0], "M/d/yyyy H:mm:ss", CultureInfo.InvariantCulture), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time").BaseUtcOffset).ToUniversalTime(),
                    EmailSubmitter = safeGet(row, 80),

                    PatientName = safeGet(row, 1),
                    OwnerName = safeGet(row, 2),
                    DateOfService = DateTimeOffset.Parse(row[3]),
                    ToEmail = template.EmailTo,
                    CcEmails = row[4].Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                    ClinicAbbreviation = safeGet(row, 81),

                    HeadNotes = safeGet(row, 5),
                    HeadOcciput = safeGet(row, 6),
                    HeadTmj = safeGet(row, 7),

                    CervicalNotes = safeGet(row, 8),
                    Cervical = [safeGet(row, 9), safeGet(row, 10), safeGet(row, 11), safeGet(row, 12), safeGet(row, 13), safeGet(row, 14), safeGet(row, 15)],

                    ThoracicNotes = safeGet(row, 16),
                    Sternum = safeGet(row, 43),
                    Thoracic = [safeGet(row, 17), safeGet(row, 18), safeGet(row, 19), safeGet(row, 20), safeGet(row, 21), safeGet(row, 22), safeGet(row, 23), safeGet(row, 24), safeGet(row, 25), safeGet(row, 26), safeGet(row, 27), safeGet(row, 28), safeGet(row, 29)],

                    Ribs = [safeGet(row, 30), safeGet(row, 31), safeGet(row, 32), safeGet(row, 33), safeGet(row, 34), safeGet(row, 35), safeGet(row, 36), safeGet(row, 37), safeGet(row, 38), safeGet(row, 39), safeGet(row, 40), safeGet(row, 41), safeGet(row, 42)],

                    LumbarNotes = safeGet(row, 44),
                    Lumbar = [safeGet(row, 45), safeGet(row, 46), safeGet(row, 47), safeGet(row, 48), safeGet(row, 49), safeGet(row, 50), safeGet(row, 51)],

                    SacrumNotes = safeGet(row, 52),
                    SacrumBase = safeGet(row, 53),
                    SacrumApex = safeGet(row, 54),

                    PelvicNotes = safeGet(row, 55),
                    PelvicLeft = safeGet(row, 56),
                    PelvicRight = safeGet(row, 57),
                    PelvicTraction = safeGet(row, 58),

                    LeftForelimbNotes = safeGet(row, 59),
                    LeftForelimbScapula = safeGet(row, 60),
                    LeftForelimbHumerus = safeGet(row, 61),
                    LeftForelimbUlna = safeGet(row, 62),
                    LeftForelimbRadius = safeGet(row, 63),
                    LeftForelimbCarpus = safeGet(row, 64),
                    LeftForelimbMetatarsalsPhalanges = safeGet(row, 65),

                    RightForelimbNotes = safeGet(row, 66),
                    RightForelimbScapula = safeGet(row, 67),
                    RightForelimbHumerus = safeGet(row, 68),
                    RightForelimbUlna = safeGet(row, 69),
                    RightForelimbRadius = safeGet(row, 70),
                    RightForelimbCarpus = safeGet(row, 71),
                    RightForelimbMetatarsalsPhalanges = safeGet(row, 72),

                    LeftRearLimbNotes = safeGet(row, 73),
                    LeftRearLimb = safeGet(row, 74),

                    RightRearLimbNotes = safeGet(row, 75),
                    RightRearLimb = safeGet(row, 76),

                    CoccygealNotes = safeGet(row, 77),
                    Coccygeal = safeGet(row, 78),

                    Other = safeGet(row, 79),
                };

                // Validate submitter before recording.
                // Historically, old rows did not have the email captured.
                // So only check for allowed submitters if both have a value.
                if (!string.IsNullOrWhiteSpace(input.EmailSubmitter) && !string.IsNullOrWhiteSpace(template.AllowedSubmitterEmails))
                {
                    if (template.AllowedSubmitterEmails.Contains(input.EmailSubmitter))
                    {
                        this.logger.LogInformation($"{input.EmailSubmitter} is allowed to use this form.");
                    }
                    else
                    {
                        this.logger.LogWarning($"{input.EmailSubmitter} is not allowed to use this form.");
                        _ = nsContext.ChiroRecords.Add(new ChiroRecord
                        {
                            Source = template.SpreadsheetId,
                            RowId = id,
                            DataEnteredAt = input.RowCreatedAt,
                        });
                        _ = await nsContext.SaveChangesAsync(cancellationToken);
                        break;
                    }
                }

                // Save the record and schedule the orchestration.
                ChiroRecord record = new()
                {
                    Source = template.SpreadsheetId,
                    RowId = id,
                    DataEnteredAt = input.RowCreatedAt,
                    InputJson = JsonSerializer.Serialize(input),
                };
                _ = nsContext.ChiroRecords.Add(record);
                _ = await nsContext.SaveChangesAsync(cancellationToken);

                TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
                OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                    typeof(DurableTaskImpl.Orchestrators.ChiroCanineGenerationOrchestrator),
                    instanceId: $"{id.MakeHumanFriendly()}_{template.Id}",
                    record.RowId);
                this.logger.LogInformation($"Created orchestration {instance.InstanceId} to generate the pdf.");
                break;
            }
        }
    }
}
