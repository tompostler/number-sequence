using DurableTask.Core;
using System.Threading.Tasks;

namespace number_sequence.DurableTaskImpl.Orchestrators
{
    public sealed class LatexGenerationOrchestrator : TaskOrchestration<string, string>
    {
        public override async Task<string> RunTask(OrchestrationContext context, string input)
        {
            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.GeneratePdfFromLatexActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input);

            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.EmailPdfForLatexActivity),
                ServiceProviderOrchestrationExtensions.DefaultLightExponentialRetryOptions,
                input);

            return default;
        }
    }
}
