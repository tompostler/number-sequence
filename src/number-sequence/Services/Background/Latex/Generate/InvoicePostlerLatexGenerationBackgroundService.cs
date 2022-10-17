using Azure.Storage.Blobs;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using number_sequence.DataAccess;
using number_sequence.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.EntityFrameworkCore;
using number_sequence.Models;
using System.Linq;
using TcpWtf.NumberSequence.Contracts.Invoicing;
using System.Text;

namespace number_sequence.Services.Background.Latex.Generate
{
    public sealed class InvoicePostlerLatexGenerationBackgroundService : SqlSynchronizedBackgroundService
    {
        private readonly NsStorage nsStorage;

        public InvoicePostlerLatexGenerationBackgroundService(
            NsStorage nsStorage,
            IOptions<Options.Google> googleOptions,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<InvoicePostlerLatexGenerationBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        {
            this.nsStorage = nsStorage;
        }

        protected override TimeSpan Interval => TimeSpan.FromMinutes(15);

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Get the template information
            LatexTemplate template = await nsContext.LatexTemplates.FirstOrDefaultAsync(x => x.Id == NsStorage.C.LTBP.InvoicePostler, cancellationToken);
            if (template == default)
            {
                this.logger.LogInformation("No template defined.");
                return;
            }

            // Check if there's any invoices ready to be processed
            Invoice invoice = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .OrderBy(x => x.ModifiedDate)
                .FirstOrDefaultAsync(x => x.ReadyForProcessing && x.ProcessedAt == default, cancellationToken);
            if (invoice == default)
            {
                this.logger.LogInformation("No invoices marked ready for processing.");
                return;
            }

            // Create the new records for generating the document
            LatexDocument latexDocument = new()
            {
                Id = invoice.Id.ToString().MakeHumanFriendly() + '_' + template.Id
            };
            _ = nsContext.LatexDocuments.Add(latexDocument);

            // Copy the template blob(s) to the working directory
            BlobClient templateLatexBlob = default;
            await foreach (BlobClient templateBlob in this.nsStorage.EnumerateAllBlobsForLatexTemplateAsync(template.Id, cancellationToken))
            {
                string targetPath = $"{NsStorage.C.LBP.Input}/{templateBlob.Name.Substring((template.Id + "/").Length).Replace("template", latexDocument.Id)}";
                BlobClient targetBlob = this.nsStorage.GetBlobClientForLatexJob(latexDocument.Id, targetPath);
                this.logger.LogInformation($"Copying {templateBlob.Uri} to {targetBlob.Uri}");
                _ = await targetBlob.SyncCopyFromUriAsync(
                    templateBlob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(this.Interval * 3)),
                    cancellationToken: cancellationToken);

                if (templateBlob.Name.EndsWith("template.tex"))
                {
                    templateLatexBlob = targetBlob;
                }
            }


            // Convert the invoice into string components for the template
            string id = invoice.Id.ToString("000000");

            StringBuilder lines = new(invoice.Lines.Count * 20);
            foreach (InvoiceLine line in invoice.Lines)
            {
                // Title/Description
                _ = lines.Append(line.Title.EscapeForLatex());
                if (!string.IsNullOrWhiteSpace(line.Description))
                {
                    _ = lines.AppendLine(@" \newline");
                    _ = lines.Append(@"\textit{\color{darkgray} ");
                    _ = lines.Append(line.Description.EscapeForLatex());
                    _ = lines.Append('}');
                }
                _ = lines.AppendLine();
                _ = lines.Append('&');
                _ = lines.AppendLine();

                // Quantity
                if (string.IsNullOrWhiteSpace(line.Unit))
                {
                    _ = lines.Append(line.Quantity);
                }
                else
                {
                    // We have a unit and should format the quantity differently
                    _ = lines.Append(line.Quantity.ToString("N1"));
                    _ = lines.Append(' ');
                    _ = lines.Append(line.Unit.EscapeForLatex());
                }
                _ = lines.AppendLine();
                _ = lines.Append('&');
                _ = lines.AppendLine();

                // Price
                _ = lines.Append(@"\$");
                _ = lines.Append(line.Price.ToString("N2"));
                if (!string.IsNullOrWhiteSpace(line.Unit))
                {
                    _ = lines.Append('/');
                    _ = lines.Append(line.Unit.EscapeForLatex());
                }
                _ = lines.AppendLine();
                _ = lines.Append('&');
                _ = lines.AppendLine();

                // Amount
                _ = lines.Append(@"\$");
                _ = lines.Append((line.Quantity * line.Price).ToString("N2"));
                _ = lines.AppendLine();
                _ = lines.Append(@"\\");
                _ = lines.AppendLine();
                _ = lines.AppendLine();
            }


            // Download the template to memory to do the string replacement
            this.logger.LogInformation($"Downloading {templateLatexBlob.Uri} to memory for template application.");
            MemoryStream templateLatexBlobContents = new();
            _ = await templateLatexBlob.DownloadToAsync(templateLatexBlobContents, cancellationToken);
            templateLatexBlobContents.Position = 0;
            string templateContents = new StreamReader(templateLatexBlobContents).ReadToEnd();
            templateLatexBlobContents.Position = 0;

            // Do the string replacement
            templateContents = templateContents
                .Replace("((Title))", invoice.Title?.EscapeForLatex() ?? @"Invoice \#((Id))")
                .Replace("((BusinessName))", invoice.Business.Name?.EscapeForLatex())
                .Replace("((BusinessAddressLine1))", invoice.Business.AddressLine1?.EscapeForLatex())
                .Replace("((BusinessAddressLine2))", invoice.Business.AddressLine2?.EscapeForLatex())
                .Replace("((BusinessContact))", invoice.Business.Contact?.EscapeForLatex())
                .Replace("((Id))", id)
                .Replace("((DueDate))", invoice.DueDate.ToString("MMMM dd, yyyy"))
                .Replace("((Description))", invoice.Description?.EscapeForLatex())
                .Replace("((CustomerName))", invoice.Customer.Name?.EscapeForLatex())
                .Replace("((CustomerContact))", invoice.Customer.Contact?.EscapeForLatex())
                .Replace("((CustomerAddressLine1))", invoice.Customer.AddressLine1?.EscapeForLatex())
                .Replace("((CustomerAddressLine2))", invoice.Customer.AddressLine2?.EscapeForLatex())
                .Replace("((Total))", invoice.Total.ToString("N2"))
                .Replace("((Lines))", lines.ToString())
                ;

            // Upload back to be processed
            this.logger.LogInformation($"Uploading memory for template application to {templateLatexBlob.Uri}.");
            StreamWriter writer = new(templateLatexBlobContents);
            writer.Write(templateContents);
            writer.Flush();
            templateLatexBlobContents.Position = 0;
            _ = await templateLatexBlob.UploadAsync(templateLatexBlobContents, overwrite: true, cancellationToken);

            // Add the email request
            string subject = default;
            if (!string.IsNullOrWhiteSpace(template.SubjectTemplate))
            {
                subject = template.SubjectTemplate
                    .Replace("((Id))", id)
                    .Replace("((BusinessName))", invoice.Business.Name)
                    .Replace("((CustomerName))", invoice.Customer.Name)
                    .Replace("((Title))", invoice.Title ?? $"Invoice #{id}")
                    ;
            }
            string attachmentName = default;
            if (!string.IsNullOrWhiteSpace(template.AttachmentNameTemplate))
            {
                attachmentName = template.AttachmentNameTemplate
                    .Replace("((Id))", id)
                    .Replace("((BusinessName))", invoice.Business.Name)
                    .Replace("((CustomerName))", invoice.Customer.Name)
                    .Replace(" ", "-")
                    ;
            }
            _ = nsContext.EmailLatexDocuments.Add(
                new EmailLatexDocument
                {
                    Id = latexDocument.Id,
                    To = template.EmailTo,
                    Subject = subject,
                    AttachmentName = attachmentName
                });
            invoice.ProcessedAt = DateTimeOffset.UtcNow;

            // And save it to enable processing
            _ = await nsContext.SaveChangesAsync(cancellationToken);
        }
    }
}
