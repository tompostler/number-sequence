using DurableTask.Core;

namespace number_sequence.DurableTaskImpl.Orchestrators
{
    public sealed class ChiroEquineGenerationOrchestrator : TaskOrchestration<string, string>
    {
        public override async Task<string> RunTask(OrchestrationContext context, string rowId)
        {
            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.ChiroEquinePdfGenerationActivity),
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
