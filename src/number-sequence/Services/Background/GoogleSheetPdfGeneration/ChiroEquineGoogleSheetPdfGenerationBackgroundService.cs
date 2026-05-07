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
    public sealed class ChiroEquineGoogleSheetPdfGenerationBackgroundService : SqlSynchronizedBackgroundService
    {
        private readonly GoogleSheetDataAccess googleSheetDataAccess;

        public ChiroEquineGoogleSheetPdfGenerationBackgroundService(
            GoogleSheetDataAccess googleSheetDataAccess,
            IOptions<Options.Google> googleOptions,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ChiroEquineGoogleSheetPdfGenerationBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        {
            this.googleSheetDataAccess = googleSheetDataAccess;
        }

        protected override List<CronExpression> Crons => new()
        {
            // 15 seconds into the minute, every 15 minutes, 9AM through 10PM, Monday through Friday
            CronExpression.Parse("15 */15 9-22 * * MON-FRI", CronFormat.IncludeSeconds),
            // 15 seconds into the minute, every hour, 12AM through 9AM and 10PM through 12AM, Monday through Friday
            CronExpression.Parse("15 0 0-9,23 * * MON-FRI", CronFormat.IncludeSeconds),
            // 15 seconds into the minute, every hour, Saturday through Sunday
            CronExpression.Parse("15 0 * * * SAT-SUN", CronFormat.IncludeSeconds),
        };

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Get the template information.
            PdfTemplate template = await nsContext.PdfTemplates.FirstOrDefaultAsync(x => x.Id == NsStorage.C.PT.ChiroEquine, cancellationToken);
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
                    EmailSubmitter = safeGet(row, 92),

                    PatientName = safeGet(row, 1),
                    OwnerName = safeGet(row, 2),
                    DateOfService = DateTimeOffset.Parse(row[3]),
                    ToEmail = template.EmailTo,
                    CcEmails = row[4].Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                    ClinicAbbreviation = safeGet(row, 93),

                    HeadNotes = safeGet(row, 5),
                    HeadOcciput = safeGet(row, 6),
                    HeadTmj = safeGet(row, 7),

                    CervicalNotes = safeGet(row, 8),
                    Cervical = [safeGet(row, 9), safeGet(row, 10), safeGet(row, 11), safeGet(row, 12), safeGet(row, 13), safeGet(row, 14), safeGet(row, 15)],

                    ThoracicNotes = safeGet(row, 16),
                    Sternum = safeGet(row, 89),
                    Thoracic = [safeGet(row, 17), safeGet(row, 18), safeGet(row, 19), safeGet(row, 20), safeGet(row, 21), safeGet(row, 22), safeGet(row, 23), safeGet(row, 24), safeGet(row, 25), safeGet(row, 26), safeGet(row, 27), safeGet(row, 28), safeGet(row, 29), safeGet(row, 30), safeGet(row, 31), safeGet(row, 32), safeGet(row, 33), safeGet(row, 34)],

                    Ribs = [safeGet(row, 35), safeGet(row, 36), safeGet(row, 37), safeGet(row, 38), safeGet(row, 39), safeGet(row, 40), safeGet(row, 41), safeGet(row, 42), safeGet(row, 43), safeGet(row, 44), safeGet(row, 45), safeGet(row, 46), safeGet(row, 47), safeGet(row, 48), safeGet(row, 49), safeGet(row, 50), safeGet(row, 51), safeGet(row, 52)],

                    LumbarNotes = safeGet(row, 53),
                    Lumbar = [safeGet(row, 54), safeGet(row, 55), safeGet(row, 56), safeGet(row, 57), safeGet(row, 58), safeGet(row, 59)],
                    LumbarIntertransverse = [safeGet(row, 60), safeGet(row, 61), safeGet(row, 62)],

                    SacrumNotes = safeGet(row, 63),
                    SacrumBase = safeGet(row, 64),
                    SacrumApex = safeGet(row, 65),

                    PelvicNotes = safeGet(row, 66),
                    PelvicLeft = safeGet(row, 67),
                    PelvicRight = safeGet(row, 68),
                    PelvicTraction = safeGet(row, 90),

                    LeftForelimbNotes = safeGet(row, 69),
                    LeftForelimbScapula = safeGet(row, 70),
                    LeftForelimbHumerus = safeGet(row, 71),
                    LeftForelimbUlna = safeGet(row, 72),
                    LeftForelimbRadius = safeGet(row, 73),
                    LeftForelimbCarpus = safeGet(row, 74),
                    LeftForelimbMetatarsalsPhalanges = safeGet(row, 75),

                    RightForelimbNotes = safeGet(row, 76),
                    RightForelimbScapula = safeGet(row, 77),
                    RightForelimbHumerus = safeGet(row, 78),
                    RightForelimbUlna = safeGet(row, 79),
                    RightForelimbRadius = safeGet(row, 80),
                    RightForelimbCarpus = safeGet(row, 81),
                    RightForelimbMetatarsalsPhalanges = safeGet(row, 82),

                    LeftRearLimbNotes = safeGet(row, 83),
                    LeftRearLimb = safeGet(row, 84),

                    RightRearLimbNotes = safeGet(row, 85),
                    RightRearLimb = safeGet(row, 86),

                    Other = safeGet(row, 87),

                    CoccygealNotes = safeGet(row, 88),
                    Coccygeal = safeGet(row, 91),
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
                    typeof(DurableTaskImpl.Orchestrators.ChiroEquineGenerationOrchestrator),
                    instanceId: $"{id.MakeHumanFriendly()}_{template.Id}",
                    record.RowId);
                this.logger.LogInformation($"Created orchestration {instance.InstanceId} to generate the pdf.");
                break;
            }
        }
    }
}
