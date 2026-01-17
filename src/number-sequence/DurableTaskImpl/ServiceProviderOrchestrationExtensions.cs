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

                // Invoice (postler) generation
                .AddSingleton<TaskOrchestration, Orchestrators.InvoicePostlerGenerationOrchestrator>()
                .AddSingleton<TaskActivity, Activities.InvoicePostlerPdfGenerationActivity>()

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
