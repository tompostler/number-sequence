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
                typeof(Activities.CopyPdfForLatexForEmailingActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input ?? context.OrchestrationInstance.InstanceId);

            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.EmailPdfActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input ?? context.OrchestrationInstance.InstanceId);

            return default;
        }
    }
}
