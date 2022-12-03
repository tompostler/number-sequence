using Azure.Storage.Blobs;
using DurableTask.Core;
using Microsoft.Extensions.DependencyInjection;
using number_sequence.DataAccess;
using System.IO;
using System.Text;
using System.Threading;
using System;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts.Invoicing;
using Microsoft.Extensions.Logging;
using number_sequence.Utilities;
using number_sequence.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace number_sequence.DurableTaskImpl.Activities
{
    public sealed class InvoicePostlerLatexGenerationActivity : AsyncTaskActivity<long, string>
    {
        private readonly NsStorage nsStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<InvoicePostlerLatexGenerationActivity> logger;

        public InvoicePostlerLatexGenerationActivity(
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<InvoicePostlerLatexGenerationActivity> logger)
        {
            this.nsStorage = nsStorage;
            this.serviceProvider = serviceProvider;
            this.sentinals = sentinals;
            this.logger = logger;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, long input)
        {
            using CancellationTokenSource cts = new(TimeSpan.FromMinutes(5));
            CancellationToken cancellationToken = cts.Token;
            await this.sentinals.DBMigration.WaitForCompletionAsync(cancellationToken);

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // Get the template information
            LatexTemplate template = await nsContext.LatexTemplates.FirstOrDefaultAsync(x => x.Id == NsStorage.C.LTBP.InvoicePostler, cancellationToken);
            if (template == default)
            {
                throw new InvalidOperationException("Could not find requested template.");
            }

            // Check if there's any invoices ready to be processed
            Invoice invoice = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == input && x.ReadyForProcessing && x.ProcessedAt == default, cancellationToken);
            if (invoice == default)
            {
                throw new InvalidOperationException("Could not find invoice that was ready for processing.");
            }

            // Create the new records for generating the document
            string invoiceId = context.OrchestrationInstance.InstanceId.Split('_').First();
            LatexDocument latexDocument = new()
            {
                Id = context.OrchestrationInstance.InstanceId
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
                    templateBlob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(15)),
                    cancellationToken: cancellationToken);

                if (templateBlob.Name.EndsWith("template.tex"))
                {
                    templateLatexBlob = targetBlob;
                }
            }


            // Convert the invoice into string components for the template
            string dueDate = invoice.DueDate.ToString("MMMM dd, yyyy");

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
                _ = lines.Append(@"\$ ");
                if (line.Price < 0)
                {
                    _ = lines.Append('(');
                }
                _ = lines.Append(Math.Abs(line.Quantity * line.Price).ToString("N2"));
                if (line.Price < 0)
                {
                    _ = lines.Append(')');
                }
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
                .Replace("((Title))", string.IsNullOrEmpty(invoice.Title) ? @"Invoice \#((Id))" : invoice.Title?.EscapeForLatex())
                .Replace("((BusinessName))", invoice.Business.Name?.EscapeForLatex())
                .Replace("((BusinessAddressLine1))", invoice.Business.AddressLine1?.EscapeForLatex())
                .Replace("((BusinessAddressLine2))", invoice.Business.AddressLine2?.EscapeForLatex())
                .Replace("((BusinessContact))", invoice.Business.Contact?.EscapeForLatex())
                .Replace("((Id))", invoiceId)
                .Replace("((DueDate))", dueDate)
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
                    .Replace("((Id))", invoiceId)
                    .Replace("((CustomerName))", invoice.Customer.Name)
                    .Replace("((Title))", string.IsNullOrEmpty(invoice.Title) ? $"Invoice #{invoiceId}" : invoice.Title)
                    ;
                if (subject.Length > 128)
                {
                    subject = subject.Substring(0, 128);
                }
            }
            string attachmentName = default;
            if (!string.IsNullOrWhiteSpace(template.AttachmentNameTemplate))
            {
                attachmentName = template.AttachmentNameTemplate
                    .Replace("((Id))", invoiceId)
                    .Replace("((BusinessName))", invoice.Business.Name)
                    .Replace("((CustomerName))", invoice.Customer.Name)
                    .Replace("((Title))", string.IsNullOrEmpty(invoice.Title) ? $"Invoice #{invoiceId}" : invoice.Title)
                    .Replace(" ", "-")
                    ;
                if (attachmentName.Length > 128)
                {
                    attachmentName = string.Concat(attachmentName.AsSpan(0, 124), ".pdf");
                }
            }
            StringBuilder additionalBody = new();
            _ = additionalBody.AppendLine($"Invoice id: {invoiceId}");
            _ = additionalBody.AppendLine($"Business name: {invoice.Business.Name}");
            _ = additionalBody.AppendLine($"Customer name: {invoice.Customer.Name}");
            _ = additionalBody.AppendLine($"Due date: {dueDate}");
            _ = additionalBody.AppendLine($"Total due: $ {invoice.Total:N2}");
            _ = additionalBody.AppendLine($"Line count: {invoice.Lines.Count}");
            _ = nsContext.EmailLatexDocuments.Add(
                new EmailLatexDocument
                {
                    Id = latexDocument.Id,
                    To = template.EmailTo,
                    Subject = subject,
                    AttachmentName = attachmentName,
                    AdditionalBody = additionalBody.ToString()
                });
            invoice.ProcessedAt = DateTimeOffset.UtcNow;

            // And save it to enable processing
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return string.Empty;
        }
    }
}
