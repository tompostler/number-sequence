using Azure.Storage.Blobs;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace number_sequence.Services.Background.Latex
{
    public sealed class EmailPdfForLatexBackgroundService : SqlSynchronizedBackgroundService
    {
        private readonly EmailDataAccess emailDataAccess;
        private readonly NsStorage nsStorage;

        public EmailPdfForLatexBackgroundService(
            EmailDataAccess emailDataAccess,
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<EmailPdfForLatexBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        {
            this.emailDataAccess = emailDataAccess;
            this.nsStorage = nsStorage;
        }

        protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            // First see if there's work to do
            EmailLatexDocument emailLatexDocument = await nsContext.EmailLatexDocuments
                .OrderBy(d => d.CreatedDate)
                .Where(d => d.ProcessedAt == default)
                .FirstOrDefaultAsync(cancellationToken);
            if (emailLatexDocument == default)
            {
                this.logger.LogInformation("No work to do.");
                return;
            }

            // Then see if the corresponding work is complete
            LatexDocument latexDocument = await nsContext.LatexDocuments
                .Where(d => d.Id == emailLatexDocument.Id && d.ProcessedAt != default)
                .FirstOrDefaultAsync(cancellationToken);
            if (latexDocument == default)
            {
                this.logger.LogInformation("No completed documents to use.");
                return;
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
                return;
            }

            // Build up the message to send
            MailMessage msg = new()
            {
                Subject = emailLatexDocument.Subject,
                Body = $"Generation of PDF (id: {emailLatexDocument.Id}) was successful."
            };
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
        }
    }
}
