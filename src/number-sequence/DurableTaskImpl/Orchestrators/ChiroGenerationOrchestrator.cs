using DurableTask.Core;

namespace number_sequence.DurableTaskImpl.Orchestrators
{
    /// <summary>
    /// Shared by every species; the activity reads the species off of the recorded input.
    /// </summary>
    public sealed class ChiroGenerationOrchestrator : TaskOrchestration<string, string>
    {
        public override async Task<string> RunTask(OrchestrationContext context, string rowId)
        {
            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.ChiroPdfGenerationActivity),
                ServiceProviderOrchestrationExtensions.DefaultExponentialRetryOptions,
                rowId);

            _ = await context.ScheduleWithRetry<string>(
                typeof(Activities.EmailPdfActivity),
                ServiceProviderOrchestrationExtensions.DefaultExponentialRetryOptions,
                context.OrchestrationInstance.InstanceId);

            return default;
        }
    }
}
