using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Cronos;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace number_sequence.Services.Background
{
    public sealed class ChiroBatchSendBackgroundService : SqlSynchronizedBackgroundService
    {
        private readonly NsStorage nsStorage;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly Options.Email emailOptions;
        private readonly JsonSerializerOptions serializerOptions;

        public ChiroBatchSendBackgroundService(
            NsStorage nsStorage,
            IOptions<Options.Email> emailOptions,
            IHttpClientFactory httpClientFactory,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ChiroBatchSendBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        {
            this.nsStorage = nsStorage;
            this.httpClientFactory = httpClientFactory;
            this.emailOptions = emailOptions.Value;
            this.serializerOptions = new()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        protected override List<CronExpression> Crons =>
        [
            // On the 10th, 20th, and 30th of every month, at 10:10 AM
            CronExpression.Parse("10 10 10,20,30 * *"),
        ];

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Figure out what needs to be done yet
            List<ChiroEmailBatch> chiroEmailBatches = await nsContext.ChiroEmailBatches
                                                        .Where(x => x.ProcessedAt == default)
                                                        .ToListAsync(cancellationToken);
            this.logger.LogInformation($"{chiroEmailBatches.Count} to process.");

            // Group them up and process. Will either have straight email, or need to translate back to email from clinic.
            var batchesByEmail = chiroEmailBatches
                                    .Where(x => !string.IsNullOrWhiteSpace(x.CcEmail))
                                    .GroupBy(x => x.CcEmail)
                                    .ToDictionary(x => x.Key, x => x.ToList());

            var batchesByClinic = chiroEmailBatches
                                    .Where(x => !string.IsNullOrWhiteSpace(x.ClinicAbbreviation))
                                    .GroupBy(x => x.ClinicAbbreviation)
                                    .ToDictionary(x => x.Key, x => x.ToList());
            foreach (KeyValuePair<string, List<ChiroEmailBatch>> batchByClinic in batchesByClinic)
            {
                if (!this.emailOptions.ChiroBatchMapParsed.TryGetValue(batchByClinic.Key, out string toEmail))
                {
                    this.logger.LogError($"Clinic abbreviation [{batchByClinic.Key}] is not found in {nameof(this.emailOptions.ChiroBatchMap)}");
                    continue;
                }
                else if (batchesByEmail.TryGetValue(toEmail, out List<ChiroEmailBatch> existingBatches))
                {
                    this.logger.LogInformation($"Adding {batchByClinic.Value.Count} records from {batchByClinic.Key} to existing {toEmail} batch with {existingBatches.Count} records.");
                    existingBatches.AddRange(batchByClinic.Value);
                }
                else
                {
                    _ = batchesByEmail.TryAdd(toEmail, batchByClinic.Value);
                }
            }

            foreach (KeyValuePair<string, List<ChiroEmailBatch>> batchByEmail in batchesByEmail)
            {
                this.logger.LogInformation($"Processing {batchByEmail.Key} with {batchByEmail.Value.Count} records.");
                ChiroBatchUriPayload payload = new()
                {
                    To = string.IsNullOrEmpty(this.emailOptions.LocalDevToOverride) ? batchByEmail.Key : this.emailOptions.LocalDevToOverride,
                    Subject = string.IsNullOrEmpty(this.emailOptions.LocalDevToOverride) ? "Chiro Records" : "[LOCALDEV] Chiro Records",
                    Body = $"There are {batchByEmail.Value.Count} attached records.\nThis is an automated message. Please let us know if there are any issues.",
                };

                // If there's less than 7, just throw them on as individual attachements.
                if (batchByEmail.Value.Count < 7)
                {
                    foreach (ChiroEmailBatch record in batchByEmail.Value)
                    {
                        BlobClient blobClient = this.nsStorage.GetBlobClient(record);
                        BlobDownloadResult result = await blobClient.DownloadContentAsync(cancellationToken);
                        payload.Attachments.Add(new()
                        {
                            Name = Path.GetFileName(blobClient.Name),
                            ContentBytes = Convert.ToBase64String(result.Content),
                        });

                        record.ProcessedAt = DateTimeOffset.UtcNow;
                    }
                }

                // If there's more than that, zip them together.
                else
                {
                    using MemoryStream ms = new();
                    using (ZipArchive zip = new(ms, ZipArchiveMode.Create, leaveOpen: true))
                    {
                        foreach (ChiroEmailBatch record in batchByEmail.Value)
                        {
                            BlobClient blobClient = this.nsStorage.GetBlobClient(record);
                            BlobDownloadResult result = await blobClient.DownloadContentAsync(cancellationToken);
                            ZipArchiveEntry entry = zip.CreateEntry(Path.GetFileName(blobClient.Name), CompressionLevel.SmallestSize);
                            using Stream entryStream = entry.Open();
                            await result.Content.ToStream().CopyToAsync(entryStream, cancellationToken);

                            record.ProcessedAt = DateTimeOffset.UtcNow;
                        }
                    }
                    payload.Attachments.Add(new()
                    {
                        Name = "records-batch.zip",
                        ContentBytes = Convert.ToBase64String(ms.ToArray()),
                    });
                }

                // Send it off.
                using HttpClient httpClient = this.httpClientFactory.CreateClient();
                HttpResponseMessage response = await httpClient.PostAsync(
                    this.emailOptions.ChiroBatchUri,
                    new StringContent(JsonSerializer.Serialize(payload, this.serializerOptions), Encoding.UTF8, "application/json"),
                    cancellationToken);
                this.logger.LogInformation($"Response: {response.StatusCode} {await response.Content.ReadAsStringAsync(cancellationToken)}");
                _ = response.EnsureSuccessStatusCode();

                // And save the processing if it sent successfully.
                _ = await nsContext.SaveChangesAsync(cancellationToken);
            }
        }

        private sealed class ChiroBatchUriPayload
        {
            public string To { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public List<Attachment> Attachments { get; set; } = new();
            public sealed class Attachment
            {
                public string Name { get; set; }
                public string ContentBytes { get; set; }
            }
        }
    }
}
