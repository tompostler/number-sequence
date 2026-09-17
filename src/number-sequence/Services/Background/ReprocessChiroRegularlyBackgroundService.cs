using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using System.Text.Json;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Services.Background
{
    /// <summary>
    /// A chiro pdf generation orchestration only retries the activity itself, and it is only ever created once, at the point the record is recorded.
    /// If every attempt in that window fails, the orchestration ends up in a terminal failed state and nothing ever asks it to try again - the record just sits with <see cref="ChiroRecord.ProcessedAt"/> unset forever.
    /// 
    /// This service is what gives a stuck record effectively unlimited retries.
    /// On an interval, it creates a fresh orchestration for anything still unprocessed that's old enough that the original orchestration's own retry window has definitely elapsed.
    /// <see cref="ChiroRecord.ProcessAttempt"/> is bumped first so the new orchestration gets a distinct instance id via <see cref="ChiroOrchestrationNaming"/> instead of colliding with the failed one it's replacing.
    /// </summary>
    public sealed class ReprocessChiroRegularlyBackgroundService : SqlSynchronizedBackgroundService
    {
        public ReprocessChiroRegularlyBackgroundService(
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ReprocessChiroRegularlyBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        { }

        protected override TimeSpan? Interval => TimeSpan.FromHours(1);

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Long enough that the original orchestration's own retries (~20 minutes, see DefaultExponentialRetryOptions) have definitely finished one way or the other.
            DateTimeOffset oneHourAgo = DateTimeOffset.UtcNow.AddHours(-1);

            // InputJson is null for records deliberately never processed (an unrecognized submitter on the google sheet ingestion path records the row so it isn't re-read as new, but never schedules generation).
            List<ChiroRecord> recordsNeedingReprocessing = await nsContext.ChiroRecords
                                                            .Where(x =>
                                                                x.ProcessedAt == null
                                                                && x.InputJson != null
                                                                && x.RecordedAt < oneHourAgo)
                                                            .ToListAsync(cancellationToken);
            foreach (ChiroRecord recordNeedingReprocessing in recordsNeedingReprocessing)
            {
                recordNeedingReprocessing.ProcessAttempt += 1;

                ChiroInput chiroInput = JsonSerializer.Deserialize<ChiroInput>(recordNeedingReprocessing.InputJson);
                ChiroSpeciesDefinition species = ChiroSpeciesDefinition.Get(chiroInput.Species);

                TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
                OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                    typeof(DurableTaskImpl.Orchestrators.ChiroGenerationOrchestrator),
                    instanceId: ChiroOrchestrationNaming.InstanceId(recordNeedingReprocessing.RowId, species.TemplateId, recordNeedingReprocessing.ProcessAttempt),
                    recordNeedingReprocessing.RowId);
                this.logger.LogInformation($"Created orchestration {instance.InstanceId} to retry pdf generation (attempt {recordNeedingReprocessing.ProcessAttempt}) for ChiroRecord {recordNeedingReprocessing.RowId}.");

                _ = await nsContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
