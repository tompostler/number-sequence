using DurableTask.Core;

namespace number_sequence.DurableTaskImpl.Orchestrators
{
    public sealed class LedgerStatementGenerationOrchestrator : TaskOrchestration<string, long>
    {
        public override async Task<string> RunTask(OrchestrationContext context, long input)
        {
            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.LedgerStatementPdfGenerationActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input);

            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.EmailPdfActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                context.OrchestrationInstance.InstanceId);

            return default;
        }
    }
}
