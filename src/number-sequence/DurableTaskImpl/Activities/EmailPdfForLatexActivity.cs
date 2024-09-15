using Azure.Storage.Blobs;
using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using System.Net.Mail;

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
            EmailDocument emailDocument = await nsContext.EmailDocuments
                .FirstOrDefaultAsync(x => x.Id == input && x.ProcessedAt == default, cancellationToken);
            if (emailDocument == default)
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
                    Subject = emailDocument.Subject,
                    Body = $"Generation of PDF (id: {emailDocument.Id}) was not successful."
                };
                failedMessage.To.Add(emailDocument.To);

                await this.emailDataAccess.SendEmailAsync(failedMessage, cancellationToken);
                emailDocument.ProcessedAt = DateTimeOffset.UtcNow;
                _ = await nsContext.SaveChangesAsync(cancellationToken);
                return default;
            }

            // Build up the message to send
            MailMessage msg = new()
            {
                Subject = emailDocument.Subject,
                Body = $"Generation of PDF (id: {emailDocument.Id}) was successful."
            };
            if (!string.IsNullOrWhiteSpace(emailDocument.AdditionalBody))
            {
                msg.Body += "\n\nAdditional information:\n";
                msg.Body += emailDocument.AdditionalBody;
            }
            msg.To.Add(emailDocument.To);
            if (!string.IsNullOrWhiteSpace(emailDocument.CC))
            {
                foreach (string emailAddress in emailDocument.CC.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    msg.CC.Add(new MailAddress(emailAddress));
                }
            }

            // Download the pdf from the storage and attach it to the email
            string pdfBlobPath = $"{NsStorage.C.LBP.Output}/{emailDocument.Id}.pdf";
            this.logger.LogInformation($"Downloading pdf from {pdfBlobPath}");
            BlobClient pdfBlobClient = this.nsStorage.GetBlobClientForLatexJob(emailDocument.Id, pdfBlobPath);
            MemoryStream ms = new();
            _ = await pdfBlobClient.DownloadToAsync(ms, cancellationToken);
            ms.Position = 0;
            msg.Attachments.Add(new Attachment(ms, EnsureEndsWithPdf(emailDocument.AttachmentName ?? emailDocument.Id)));

            // Send it and mark as completed
            await this.emailDataAccess.SendEmailAsync(msg, cancellationToken);
            emailDocument.ProcessedAt = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return default;
        }

        private static string EnsureEndsWithPdf(string input) => input.EndsWith(".pdf") ? input : input + ".pdf";
    }
}
