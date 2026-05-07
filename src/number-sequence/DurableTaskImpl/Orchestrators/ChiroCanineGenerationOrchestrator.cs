using DurableTask.Core;

namespace number_sequence.DurableTaskImpl.Orchestrators
{
    public sealed class ChiroCanineGenerationOrchestrator : TaskOrchestration<string, string>
    {
        public override async Task<string> RunTask(OrchestrationContext context, string rowId)
        {
            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.ChiroCaninePdfGenerationActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                rowId);

            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.EmailPdfActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                context.OrchestrationInstance.InstanceId);

            return default;
        }
    }
}
