using DurableTask.Core;
using System.Threading.Tasks;

namespace number_sequence.DurableTask.Orchestrators
{
    public sealed class InvoicePostlerOrchestrator : TaskOrchestration<string, string>
    {
        public override async Task<string> RunTask(OrchestrationContext context, string input)
        {
            input = await context.ScheduleWithRetry<string>(
                typeof(Activities.GeneratePdfFromLatexActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input);

            input = await context.ScheduleWithRetry<string>(
                typeof(Activities.EmailPdfForLatexActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input);

            return input;
        }
    }
}
