using DurableTask.Core;

namespace number_sequence.DurableTaskImpl.Orchestrators
{
    public sealed class LatexGenerationOrchestrator : TaskOrchestration<string, string>
    {
        public override async Task<string> RunTask(OrchestrationContext context, string input)
        {
            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.GeneratePdfFromLatexActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input ?? context.OrchestrationInstance.InstanceId);

            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.EmailPdfForLatexActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input ?? context.OrchestrationInstance.InstanceId);

            return default;
        }
    }
}
