using DurableTask.Core;
using DurableTask.SqlServer;
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
        private readonly Type[] orchestratorTypes;
        private readonly IEnumerable<TaskActivity> activities;
        private readonly ILoggerFactory loggerFactory;
        private readonly Sentinals sentinals;
        private readonly ILogger<DurableOrchestrationWorkerBackgroundService> logger;

        public DurableOrchestrationWorkerBackgroundService(
            IOptions<Options.Sql> sqlOptions,
            IEnumerable<TaskOrchestration> orchestrators,
            IEnumerable<TaskActivity> activities,
            ILoggerFactory loggerFactory,
            Sentinals sentinals,
            ILogger<DurableOrchestrationWorkerBackgroundService> logger)
        {
            SqlOrchestrationServiceSettings sqlOrchestrationServiceSettings = new(sqlOptions.Value.ConnectionString)
            {
                LoggerFactory = loggerFactory,
                MaxActiveOrchestrations = 1,
                MaxConcurrentActivities = 1,

                MaxActivityPollingInterval = TimeSpan.FromMinutes(5),
                MaxOrchestrationPollingInterval = TimeSpan.FromMinutes(5),
            };
            this.sqlOrchestrationService = new(sqlOrchestrationServiceSettings);

            this.orchestratorTypes = orchestrators.Select(x => x.GetType()).ToArray();
            this.activities = activities;
            this.loggerFactory = loggerFactory;
            this.sentinals = sentinals;
            this.logger = logger;
        }

        private TaskHubWorker worker;

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            try
            {
                this.logger.LogInformation("Setting up durable orchestration background service.");

                await this.sqlOrchestrationService.CreateIfNotExistsAsync();
                this.logger.LogInformation("Created schema (if not exists)");

                this.worker = new(this.sqlOrchestrationService, this.loggerFactory);
                this.logger.LogInformation("Created worker");

                _ = this.worker.AddTaskOrchestrations(this.orchestratorTypes);
                this.logger.LogInformation($"Added {this.orchestratorTypes.Length} orchestrators: {string.Join(",", this.orchestratorTypes.Select(x => x.Name))}");
                if (this.worker.TaskOrchestrationDispatcher != null)
                {
                    this.worker.TaskOrchestrationDispatcher.IncludeDetails = true;
                    this.worker.TaskOrchestrationDispatcher.IncludeParameters = true;
                }

                _ = this.worker.AddTaskActivities(this.activities.ToArray());
                this.logger.LogInformation($"Added {this.activities.Count()} activities: {string.Join(",", this.activities.Select(x => x.GetType().Name))}");
                if (this.worker.TaskActivityDispatcher != null)
                {
                    this.worker.TaskActivityDispatcher.IncludeDetails = true;
                }

                _ = await this.worker.StartAsync();
                this.logger.LogInformation("Started worker");

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
