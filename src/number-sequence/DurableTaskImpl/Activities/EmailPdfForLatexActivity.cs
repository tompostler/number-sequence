using Azure.Storage.Blobs;
using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using System;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace number_sequence.DurableTaskImpl.Activities
{
    public sealed class EmailPdfForLatexActivity : AsyncTaskActivity<string, string>
    {
        private readonly EmailDataAccess emailDataAccess;
        private readonly NsStorage nsStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<EmailPdfForLatexActivity> logger;
        private readonly TelemetryClient telemetryClient;

        public EmailPdfForLatexActivity(
            EmailDataAccess emailDataAccess,
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<EmailPdfForLatexActivity> logger,
            TelemetryClient telemetryClient)
        {
            this.emailDataAccess = emailDataAccess;
            this.nsStorage = nsStorage;
            this.serviceProvider = serviceProvider;
            this.sentinals = sentinals;
            this.logger = logger;
            this.telemetryClient = telemetryClient;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, string input)
        {
            // Basic setup
            using IOperationHolder<RequestTelemetry> op = this.telemetryClient.StartOperation<RequestTelemetry>(
                this.GetType().FullName,
                operationId: context.OrchestrationInstance.ExecutionId,
                parentOperationId: context.OrchestrationInstance.InstanceId);
            using CancellationTokenSource cts = new(TimeSpan.FromMinutes(5));
            CancellationToken cancellationToken = cts.Token;
            await this.sentinals.DBMigration.WaitForCompletionAsync(cancellationToken);

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // First see if there's work to do
            EmailLatexDocument emailLatexDocument = await nsContext.EmailLatexDocuments
                .FirstOrDefaultAsync(x => x.Id == input && x.ProcessedAt == default, cancellationToken);
            if (emailLatexDocument == default)
            {
                throw new InvalidOperationException("Work was requested, but the email record was not found.");
            }

            // Then see if the corresponding work is complete
            LatexDocument latexDocument = await nsContext.LatexDocuments
                .FirstOrDefaultAsync(x => x.Id == input && x.ProcessedAt != default, cancellationToken);
            if (latexDocument == default)
            {
                throw new InvalidOperationException("Work was requested, but the document was not found.");
            }

            // If the latex document generation was not successful, then send the notification email only to the To address
            if (latexDocument.Successful == false)
            {
                MailMessage failedMessage = new()
                {
                    Subject = emailLatexDocument.Subject,
                    Body = $"Generation of PDF (id: {emailLatexDocument.Id}) was not successful."
                };
                failedMessage.To.Add(emailLatexDocument.To);

                await this.emailDataAccess.SendEmailAsync(failedMessage, cancellationToken);
                emailLatexDocument.ProcessedAt = DateTimeOffset.UtcNow;
                _ = await nsContext.SaveChangesAsync(cancellationToken);
                return default;
            }

            // Build up the message to send
            MailMessage msg = new()
            {
                Subject = emailLatexDocument.Subject,
                Body = $"Generation of PDF (id: {emailLatexDocument.Id}) was successful."
            };
            if (!string.IsNullOrWhiteSpace(emailLatexDocument.AdditionalBody))
            {
                msg.Body += "\n\nAdditional information:\n";
                msg.Body += emailLatexDocument.AdditionalBody;
            }
            msg.To.Add(emailLatexDocument.To);
            if (!string.IsNullOrWhiteSpace(emailLatexDocument.CC))
            {
                foreach (string emailAddress in emailLatexDocument.CC.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    msg.CC.Add(new MailAddress(emailAddress));
                }
            }

            // Download the pdf from the storage and attach it to the email
            string pdfBlobPath = $"{NsStorage.C.LBP.Output}/{emailLatexDocument.Id}.pdf";
            this.logger.LogInformation($"Downloading pdf from {pdfBlobPath}");
            BlobClient pdfBlobClient = this.nsStorage.GetBlobClientForLatexJob(emailLatexDocument.Id, pdfBlobPath);
            MemoryStream ms = new();
            _ = await pdfBlobClient.DownloadToAsync(ms, cancellationToken);
            ms.Position = 0;
            msg.Attachments.Add(new Attachment(ms, emailLatexDocument.AttachmentName ?? $"{emailLatexDocument.Id}.pdf"));

            // Send it and mark as completed
            await this.emailDataAccess.SendEmailAsync(msg, cancellationToken);
            emailLatexDocument.ProcessedAt = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return default;
        }
    }
}
