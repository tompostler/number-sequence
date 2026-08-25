using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Utilities;
using System.Diagnostics;

namespace number_sequence.Services.Background
{
    /// <summary>
    /// One-time migration: copies pdfs for pre-QuestPDF <see cref="Models.EmailDocument"/> rows out of the
    /// retired "latex" blob container - where the old latex pipeline left them - into the "pdf" container
    /// where every reader (<see cref="NsStorage.GetBlobClient(Models.EmailDocument)"/>, the pdf-status page,
    /// <see cref="FileLengthBackfillBackgroundService"/>) expects to find them today.
    /// <see cref="Models.ChiroEmailBatch"/> was introduced after the QuestPDF cutover, so it never has rows
    /// stuck in the latex container and isn't handled here.
    /// Delete this service (and <see cref="NsStorage.GetLegacyLatexPdfBlobClient"/>) once nothing matches its
    /// query anymore.
    /// </summary>
    public sealed class LatexPdfMigrationBackgroundService : SqlSynchronizedBackgroundService
    {
        // Commit 7163fe1, 2024-09-15: postler invoice generation moved off latex onto QuestPDF, the last of the
        // document types to switch. Anything processed before this was emailed straight out of the latex
        // container without ever being copied into "pdf" - see EmailPdfForLatexBackgroundService (removed in
        // 70da9ae) - so it's the correct migration cutover, not just a performance filter.
        private static readonly DateTimeOffset QuestPdfCutover = new(2024, 9, 15, 0, 0, 0, TimeSpan.Zero);

        private readonly NsStorage nsStorage;

        public LatexPdfMigrationBackgroundService(
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            NsStorage nsStorage,
            ILogger<LatexPdfMigrationBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        {
            this.nsStorage = nsStorage;
        }

        protected override TimeSpan? Interval => TimeSpan.FromMinutes(1);

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            const int batchSize = 25;
            var sw = Stopwatch.StartNew();

            List<Models.EmailDocument> emailDocuments = await nsContext.EmailDocuments
                                                            .Where(x => x.FileLength == 0 && x.ProcessedAt != null && x.CreatedDate < QuestPdfCutover)
                                                            .OrderBy(x => Guid.NewGuid())
                                                            .Take(batchSize)
                                                            .ToListAsync(cancellationToken);

            int migrated = 0;
            foreach (Models.EmailDocument emailDocument in emailDocuments)
            {
                if (await this.TryMigrateAsync(emailDocument, cancellationToken))
                {
                    migrated++;
                }
            }

            this.logger.LogInformation($"Migrated {migrated}/{emailDocuments.Count} pdfs out of the latex container after {sw.Elapsed}.");

            _ = await nsContext.SaveChangesAsync(cancellationToken);
        }

        private async Task<bool> TryMigrateAsync(Models.EmailDocument emailDocument, CancellationToken cancellationToken)
        {
            BlobClient legacyBlobClient = this.nsStorage.GetLegacyLatexPdfBlobClient(emailDocument.Id);
            BlobClient targetBlobClient = this.nsStorage.GetBlobClient(emailDocument);

            try
            {
                // Blob-to-blob within the same storage account, same trick the old CopyPdfForLatexForEmailingActivity used.
                _ = await targetBlobClient.SyncCopyFromUriAsync(
                    legacyBlobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1)),
                    cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                this.logger.LogWarning($"Legacy blob {legacyBlobClient.Uri.AbsoluteUri.Split('?').First()} not found, skipping.");
                return false;
            }

            Response<BlobProperties> properties = await targetBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            emailDocument.FileLength = properties.Value.ContentLength;
            this.logger.LogInformation($"Migrated {legacyBlobClient.Uri.AbsoluteUri.Split('?').First()} to {targetBlobClient.Uri.AbsoluteUri.Split('?').First()} ({emailDocument.FileLength} bytes).");
            return true;
        }
    }
}
