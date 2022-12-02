using DurableTask.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace number_sequence.DurableTask
{
    public static class ServiceProviderOrchestrationExtensions
    {
        public static IServiceCollection AddDurableOrchestrations(IServiceCollection services)
            => services
                // Orchestrators, and their specific activities
                .AddSingleton<TaskOrchestration, Orchestrators.InvoicePostlerOrchestrator>()

                // Shared activities
                .AddSingleton<TaskActivity, Activities.GeneratePdfFromLatexActivity>()
                .AddSingleton<TaskActivity, Activities.EmailPdfForLatexActivity>()
            ;

        public static RetryOptions DefaultLightExponentialRetryOptions => new(TimeSpan.FromSeconds(5), 3) { BackoffCoefficient = 1.7 };
    }
}
