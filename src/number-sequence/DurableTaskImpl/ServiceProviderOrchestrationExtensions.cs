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

                // Chiro generation, shared by every species
                .AddSingleton<TaskOrchestration, Orchestrators.ChiroGenerationOrchestrator>()
                .AddSingleton<TaskActivity, Activities.ChiroPdfGenerationActivity>()

                // Shared activities
                .AddSingleton<TaskActivity, Activities.EmailPdfActivity>()
            ;

        public static RetryOptions DefaultExponentialRetryOptions => new(firstRetryInterval: TimeSpan.FromSeconds(5), maxNumberOfAttempts: 6)
        {
            BackoffCoefficient = 1.9,
            MaxRetryInterval = TimeSpan.FromMinutes(3),
            RetryTimeout = TimeSpan.FromMinutes(20),
        };
    }
}
