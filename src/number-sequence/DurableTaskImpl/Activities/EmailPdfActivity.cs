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
    public sealed class EmailPdfActivity : AsyncTaskActivity<string, string>
    {
        private readonly EmailDataAccess emailDataAccess;
        private readonly NsStorage nsStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<EmailPdfActivity> logger;
        private readonly TelemetryClient telemetryClient;

        public EmailPdfActivity(
            EmailDataAccess emailDataAccess,
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<EmailPdfActivity> logger,
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
            BlobClient pdfBlobClient = this.nsStorage.GetBlobClient(emailDocument);
            this.logger.LogInformation($"Downloading pdf from {pdfBlobClient.Uri.AbsoluteUri.Split('?').First()}");
            MemoryStream ms = new();
            _ = await pdfBlobClient.DownloadToAsync(ms, cancellationToken);
            ms.Position = 0;
            msg.Attachments.Add(new Attachment(ms, Path.GetFileName(pdfBlobClient.Name)));

            // Send it and mark as completed
            await this.emailDataAccess.SendEmailAsync(msg, cancellationToken);
            emailDocument.ProcessedAt = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return default;
        }
    } 
}
