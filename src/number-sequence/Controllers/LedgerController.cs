using DurableTask.Core;
using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using number_sequence.Utilities;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]"), RequiresToken(AccountRoles.Ledger)]
    public sealed partial class LedgerController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<LedgerController> logger;

        public LedgerController(
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<LedgerController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.sentinals = sentinals;
            this.logger = logger;
        }

        private async Task TriggerInvoicePdfAsync(Invoice invoice, CancellationToken cancellationToken)
        {
            invoice.ProccessAttempt += 1;
            TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
            OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                typeof(DurableTaskImpl.Orchestrators.LedgerInvoiceGenerationOrchestrator),
                instanceId: $"{invoice.FriendlyId}_invoice",
                invoice.Id);
            this.logger.LogInformation($"Created orchestration {instance.InstanceId} to generate the pdf.");
        }

        private async Task TriggerStatementPdfAsync(Statement statement, CancellationToken cancellationToken)
        {
            statement.ProccessAttempt += 1;
            TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
            OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                typeof(DurableTaskImpl.Orchestrators.LedgerStatementGenerationOrchestrator),
                instanceId: $"{statement.FriendlyId}_statement",
                statement.Id);
            this.logger.LogInformation($"Created orchestration {instance.InstanceId} to generate the pdf.");
        }
    }
}
