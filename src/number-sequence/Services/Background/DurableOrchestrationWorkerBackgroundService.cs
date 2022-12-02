using DurableTask.Core;
using DurableTask.SqlServer;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using number_sequence.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace number_sequence.Services.Background
{
    public sealed class DurableOrchestrationWorkerBackgroundService : IHostedService
    {
        private readonly SqlOrchestrationService sqlOrchestrationService;
        private readonly IEnumerable<Orchestrations.IOrchestrator> orchestrators;
        private readonly IEnumerable<TaskActivity> activities;
        private readonly ILoggerFactory loggerFactory;
        private readonly Sentinals sentinals;
        private readonly ILogger<DurableOrchestrationWorkerBackgroundService> logger;
        private readonly TelemetryClient telemetryClient;

        public DurableOrchestrationWorkerBackgroundService(
            IOptions<Options.Sql> sqlOptions,
            IEnumerable<Orchestrations.IOrchestrator> orchestrators,
            IEnumerable<TaskActivity> activities,
            ILoggerFactory loggerFactory,
            Sentinals sentinals,
            ILogger<DurableOrchestrationWorkerBackgroundService> logger,
            TelemetryClient telemetryClient)
        {
            SqlOrchestrationServiceSettings sqlOrchestrationServiceSettings = new(sqlOptions.Value.ConnectionString)
            {
                LoggerFactory = loggerFactory,
                MaxActiveOrchestrations = 1,
                MaxConcurrentActivities = 1,
            };
            this.sqlOrchestrationService = new(sqlOrchestrationServiceSettings);

            this.orchestrators = orchestrators;
            this.activities = activities;
            this.loggerFactory = loggerFactory;
            this.sentinals = sentinals;
            this.logger = logger;
            this.telemetryClient = telemetryClient;
        }

        private TaskHubWorker worker;

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            try
            {
                using IOperationHolder<RequestTelemetry> op = this.telemetryClient.StartOperation<RequestTelemetry>(this.GetType().FullName);
                this.logger.LogInformation("Setting up durable orchestration background service.");
                await this.sqlOrchestrationService.CreateIfNotExistsAsync();

                this.worker = new(this.sqlOrchestrationService, this.loggerFactory);

                _ = this.worker.AddTaskOrchestrations(this.orchestrators.Select(x => x.GetType()).ToArray());
                this.worker.TaskOrchestrationDispatcher.IncludeDetails = true;
                this.worker.TaskOrchestrationDispatcher.IncludeParameters = true;

                _ = this.worker.AddTaskActivities(this.activities.ToArray());
                this.worker.TaskActivityDispatcher.IncludeDetails = true;

                _ = await this.worker.StartAsync();

                TaskHubClient client = new(this.sqlOrchestrationService, default, this.loggerFactory);
                this.sentinals.DurableOrchestrationClient.SignalCompletion(client);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Could not initialize durable orchestration worker.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => this.worker.StopAsync();
    }
}
