using DurableTask.Core;

namespace number_sequence.DurableTaskImpl
{
    public static class ServiceProviderOrchestrationExtensions
    {
        public static IServiceCollection AddDurableOrchestrations(IServiceCollection services)
            => services

                //
                // Orchestrators, and their specific activities
                //

                // Invoice generation
                .AddSingleton<TaskOrchestration, Orchestrators.LedgerInvoiceGenerationOrchestrator>()
                .AddSingleton<TaskActivity, Activities.LedgerInvoicePdfGenerationActivity>()

                // Statement generation
                .AddSingleton<TaskOrchestration, Orchestrators.LedgerStatementGenerationOrchestrator>()
                .AddSingleton<TaskActivity, Activities.LedgerStatementPdfGenerationActivity>()

                // Chiro (canine) generation
                .AddSingleton<TaskOrchestration, Orchestrators.ChiroCanineGenerationOrchestrator>()
                .AddSingleton<TaskActivity, Activities.ChiroCaninePdfGenerationActivity>()

                // Chiro (equine) generation
                .AddSingleton<TaskOrchestration, Orchestrators.ChiroEquineGenerationOrchestrator>()
                .AddSingleton<TaskActivity, Activities.ChiroEquinePdfGenerationActivity>()

                // Shared activities
                .AddSingleton<TaskActivity, Activities.EmailPdfActivity>()
            ;

        public static RetryOptions DefaultLightExponentialRetryOptions => new(TimeSpan.FromSeconds(5), 3) { BackoffCoefficient = 1.7 };
    }
}
