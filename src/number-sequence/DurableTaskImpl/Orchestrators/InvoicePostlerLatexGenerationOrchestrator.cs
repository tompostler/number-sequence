using DurableTask.Core;

namespace number_sequence.DurableTaskImpl.Orchestrators
{
    public sealed class InvoicePostlerLatexGenerationOrchestrator : TaskOrchestration<string, long>
    {
        public override async Task<string> RunTask(OrchestrationContext context, long input)
        {
            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.InvoicePostlerLatexGenerationActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input);

            _ = await context.CreateSubOrchestrationInstanceWithRetry<string>(
                typeof(LatexGenerationOrchestrator),
                instanceId: context.OrchestrationInstance.InstanceId + ":1",
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input: context.OrchestrationInstance.InstanceId);

            return default;
        }
    }
}
