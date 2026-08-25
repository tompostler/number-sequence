using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Cronos;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Utilities;

namespace number_sequence.Services.Background
{
    /// <summary>
    /// Backfills <see cref="Models.EmailDocument.FileLength"/>/<see cref="Models.ChiroEmailBatch.FileLength"/> for
    /// records created before that column existed, by reading the size of the pdf that's already sitting in blob
    /// storage. Best-effort: a blob that's missing (e.g. past its retention) is logged and left at 0 rather than
    /// failing the run.
    /// </summary>
    public sealed class FileLengthBackfillBackgroundService : SqlSynchronizedBackgroundService
    {
        private readonly NsStorage nsStorage;

        public FileLengthBackfillBackgroundService(
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            NsStorage nsStorage,
            ILogger<FileLengthBackfillBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        {
            this.nsStorage = nsStorage;
        }

        protected override TimeSpan? Interval => TimeSpan.FromMinutes(5);

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            const int batchSize = 25;

            List<Models.EmailDocument> emailDocuments = await nsContext.EmailDocuments
                                                            .Where(x => x.FileLength == 0)
                                                            .OrderBy(x => Guid.NewGuid())
                                                            .Take(batchSize)
                                                            .ToListAsync(cancellationToken);
            int emailDocumentsBackfilled = 0;
            foreach (Models.EmailDocument emailDocument in emailDocuments)
            {
                long? length = await this.TryGetBlobLengthAsync(this.nsStorage.GetBlobClient(emailDocument), cancellationToken);
                if (length.HasValue)
                {
                    emailDocument.FileLength = length.Value;
                    emailDocumentsBackfilled++;
                }
            }

            List<Models.ChiroEmailBatch> chiroEmailBatches = await nsContext.ChiroEmailBatches
                                                            .Where(x => x.FileLength == 0)
                                                            .OrderBy(x => Guid.NewGuid())
                                                            .Take(batchSize)
                                                            .ToListAsync(cancellationToken);
            int chiroEmailBatchesBackfilled = 0;
            foreach (Models.ChiroEmailBatch chiroEmailBatch in chiroEmailBatches)
            {
                long? length = await this.TryGetBlobLengthAsync(this.nsStorage.GetBlobClient(chiroEmailBatch), cancellationToken);
                if (length.HasValue)
                {
                    chiroEmailBatch.FileLength = length.Value;
                    chiroEmailBatchesBackfilled++;
                }
            }

            this.logger.LogInformation(
                $"Backfilled FileLength for {emailDocumentsBackfilled}/{emailDocuments.Count} email documents and " +
                $"{chiroEmailBatchesBackfilled}/{chiroEmailBatches.Count} chiro email batches.");

            _ = await nsContext.SaveChangesAsync(cancellationToken);
        }

        private async Task<long?> TryGetBlobLengthAsync(BlobClient blobClient, CancellationToken cancellationToken)
        {
            try
            {
                Response<BlobProperties> properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
                return properties.Value.ContentLength;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                this.logger.LogWarning($"Blob {blobClient.Uri.AbsoluteUri.Split('?').First()} not found, skipping.");
                return default;
            }
        }
    }
}
