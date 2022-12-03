using DurableTask.Core;
using System.Threading.Tasks;

namespace number_sequence.DurableTaskImpl.Orchestrators
{
    public sealed class InvoicePostlerOrchestrator : TaskOrchestration<string, long>
    {
        public override async Task<string> RunTask(OrchestrationContext context, long input)
        {
            string output = await context.ScheduleWithRetry<string>(
                typeof(Activities.InvoicePostlerLatexGenerationActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input);

            output = await context.ScheduleWithRetry<string>(
                typeof(Activities.GeneratePdfFromLatexActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                output);

            output = await context.ScheduleWithRetry<string>(
                typeof(Activities.EmailPdfForLatexActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                output);

            return output;
        }
    }
}
