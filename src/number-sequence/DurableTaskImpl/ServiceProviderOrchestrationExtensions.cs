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
                .AddSingleton<TaskOrchestration, Orchestrators.InvoicePostlerLatexGenerationOrchestrator>()
                .AddSingleton<TaskActivity, Activities.InvoicePostlerLatexGenerationActivity>()

                // Convert latex to a pdf and email it
                .AddSingleton<TaskOrchestration, Orchestrators.LatexGenerationOrchestrator>()
                .AddSingleton<TaskActivity, Activities.GeneratePdfFromLatexActivity>()
                .AddSingleton<TaskActivity, Activities.CopyPdfForLatexForEmailingActivity>()
                .AddSingleton<TaskActivity, Activities.EmailPdfActivity>()
            ;

        public static RetryOptions DefaultLightExponentialRetryOptions => new(TimeSpan.FromSeconds(5), 3) { BackoffCoefficient = 1.7 };
    }
}
