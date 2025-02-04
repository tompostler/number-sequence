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

        protected override List<CronExpression> Crons => new()
        {
            // On the 10th of every month, at 10:10 AM
            CronExpression.Parse("10 10 10 * *"),
        };

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Figure out what needs to be done yet
            List<ChiroEmailBatch> chiroEmailBatches = await nsContext.ChiroEmailBatches
                                                        .Where(x => x.ProcessedAt == default)
                                                        .ToListAsync(cancellationToken);
            this.logger.LogInformation($"{chiroEmailBatches.Count} to process.");

            // Group them up and process by clinic
            foreach (KeyValuePair<string, List<ChiroEmailBatch>> batchByClinic in chiroEmailBatches.GroupBy(x => x.ClinicAbbreviation).ToDictionary(x => x.Key, x => x.ToList()))
            {
                if (!this.emailOptions.ChiroBatchMapParsed.TryGetValue(batchByClinic.Key, out string toEmail))
                {
                    this.logger.LogError($"Clinic abbreviation [{batchByClinic.Key}] is not found in {nameof(this.emailOptions.ChiroBatchMap)}");
                    continue;
                }

                this.logger.LogInformation($"Processing {batchByClinic.Key} with {batchByClinic.Value.Count} records.");
                ChiroBatchUriPayload payload = new()
                {
                    To = string.IsNullOrEmpty(this.emailOptions.LocalDevToOverride) ? toEmail : this.emailOptions.LocalDevToOverride,
                    Subject = "Chiro Records",
                    Body = $"There are {batchByClinic.Value.Count} attached records.",
                };

                // If there's less than 7, just throw them on as individual attachements.
                if (batchByClinic.Value.Count < 7)
                {
                    foreach (ChiroEmailBatch record in batchByClinic.Value)
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
                        foreach (ChiroEmailBatch record in batchByClinic.Value)
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
                        Name = "records.zip",
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
