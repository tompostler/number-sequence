using Azure.Storage.Blobs;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace number_sequence.Services.Background.Latex.Generate
{
    public sealed class ChiroEquineLatexGenerationBackgroundService : SqlSynchronizedBackgroundService
    {
        private readonly GoogleSheetDataAccess googleSheetDataAccess;
        private readonly NsStorage nsStorage;

        public ChiroEquineLatexGenerationBackgroundService(
            GoogleSheetDataAccess googleSheetDataAccess,
            NsStorage nsStorage,
            IOptions<Options.Google> googleOptions,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ChiroEquineLatexGenerationBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        {
            this.googleSheetDataAccess = googleSheetDataAccess;
            this.nsStorage = nsStorage;
        }

        protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Get the template information
            LatexTemplate template = await nsContext.LatexTemplates.FirstOrDefaultAsync(x => x.Id == NsStorage.C.LTBP.ChiroEquine, cancellationToken);
            if (template == default)
            {
                this.logger.LogInformation("No template defined.");
                return;
            }

            // Get the data from the spreadsheet. The first row is the headers
            IList<IList<object>> data = await this.googleSheetDataAccess.GetAsync(template.SpreadsheetId, template.SpreadsheetRange, cancellationToken);
            IList<object> headers = data[0];
            data = data.Skip(1).ToList();

            // Only on reset or initial deployment, no data
            if (!data.Any())
            {
                this.logger.LogInformation("No rows of data.");
                return;
            }

            // Check each row of data to see if it's already been processed
            // Only process one additional row at a time
            IList<object> row = default;
            string id = default;
            LatexDocument latexDocument = default;
            for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
            {
                row = data[rowIndex];
                id = string.Join('|', row.Select(x => x.ToString())).GetSHA256();
                latexDocument = await nsContext.LatexDocuments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (latexDocument != default)
                {
                    this.logger.LogInformation($"Data row {id} ({rowIndex}) was inserted for processing at {latexDocument.CreatedDate:u} and processed {latexDocument.ProcessedAt:u}");
                    continue;
                }
                else
                {
                    this.logger.LogInformation($"Data row {id} ({rowIndex}) is new. Setting up for processing.");
                    break;
                }
            }

            // Create the new record for generating the document
            latexDocument = new()
            {
                Id = id
            };
            _ = nsContext.LatexDocuments.Add(latexDocument);

            // Copy the template blob(s) to the working directory
            await foreach (BlobClient templateBlob in this.nsStorage.EnumerateAllBlobsForLatexTemplateAsync(NsStorage.C.LTBP.ChiroEquine, cancellationToken))
            {
                string targetPath = Path.Combine(NsStorage.C.LBP.Input, templateBlob.Name.Substring((NsStorage.C.LTBP.ChiroEquine + "/").Length)).Replace("template", id);
                BlobClient targetBlob = this.nsStorage.GetBlobClientForLatexJob(id, targetPath);
                this.logger.LogInformation($"Copying {templateBlob.Uri} to {targetBlob.Uri}");
                _ = await targetBlob.SyncCopyFromUriAsync(templateBlob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(this.Interval * 3)), cancellationToken: cancellationToken);
            }

            // Convert the data row into the template


            // Add the email request
            _ = nsContext.EmailLatexDocuments.Add(
                new EmailLatexDocument
                {
                    Id = id,
                    To = template.EmailTo,
                    CC = default,
                    Subject = default,
                    AttachmentName = default
                });

            // And save it to enable processing
            //_ = await nsContext.SaveChangesAsync(cancellationToken);
        }
    }
}
