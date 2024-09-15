using Azure.Storage.Blobs;
using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;

namespace number_sequence.DurableTaskImpl.Activities
{
    public sealed class CopyPdfForLatexForEmailingActivity : AsyncTaskActivity<string, string>
    {
        private readonly NsStorage nsStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<CopyPdfForLatexForEmailingActivity> logger;
        private readonly TelemetryClient telemetryClient;

        public CopyPdfForLatexForEmailingActivity(
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<CopyPdfForLatexForEmailingActivity> logger,
            TelemetryClient telemetryClient)
        {
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

            // Copy from the latex output to the pdf container.
            string pdfBlobPath = $"{NsStorage.C.LBP.Output}/{input}.pdf";
            BlobClient latexOutputBlobClient = this.nsStorage.GetBlobClientForLatexJob(emailDocument.Id, pdfBlobPath);
            BlobClient emailPdfBlobClient = this.nsStorage.GetBlobClient(emailDocument);
            _ = await emailPdfBlobClient.SyncCopyFromUriAsync(latexOutputBlobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1)), cancellationToken: cancellationToken);

            return default;
        }
    }
}
