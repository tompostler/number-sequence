using DurableTask.Core;
using System.Threading.Tasks;

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
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                context.OrchestrationInstance.InstanceId);

            return string.Empty;
        }
    }
}
