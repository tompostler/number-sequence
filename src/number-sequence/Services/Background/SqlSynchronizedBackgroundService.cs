﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using number_sequence.DataAccess;
using number_sequence.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace number_sequence.Services.Background
{
    public abstract class SqlSynchronizedBackgroundService : BackgroundService
    {
        protected readonly IServiceProvider serviceProvider;
        protected readonly Sentinals sentinals;
        protected readonly ILogger logger;

        private readonly TelemetryClient telemetryClient;
        protected IOperationHolder<RequestTelemetry> op;

        public SqlSynchronizedBackgroundService(
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger logger,
            TelemetryClient telemetryClient)
        {
            this.serviceProvider = serviceProvider;
            this.sentinals = sentinals;
            this.logger = logger;
            this.telemetryClient = telemetryClient;
        }

        protected override sealed async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.sentinals.DBMigration.WaitForCompletionAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // Make sure there's only one instance of the service attempting to run at any time
                using (IServiceScope scope = this.serviceProvider.CreateScope())
                using (NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>())
                {
                    Models.SynchronizedBackgroundService lastExecution = await nsContext.SynchronizedBackgroundServices.SingleOrDefaultAsync(x => x.Name == this.GetType().FullName, stoppingToken);
                    if (lastExecution != default && lastExecution.LastExecuted.Add(this.Interval) > DateTimeOffset.UtcNow)
                    {
                        this.logger.LogInformation($"Last execution of {this.GetType().FullName} was {DateTimeOffset.UtcNow - lastExecution.LastExecuted} ago when the interval is {this.Interval}.");
                        DateTimeOffset nextExpectedExecution = lastExecution.LastExecuted.Add(this.Interval);
                        TimeSpan timeUntilNextExpectedExecution = nextExpectedExecution - DateTimeOffset.UtcNow;
                        var durationToSleep = TimeSpan.FromMinutes(Math.Max(5, timeUntilNextExpectedExecution.TotalMinutes));
                        this.logger.LogInformation($"Determined we should sleep for {durationToSleep}");
                        await Task.Delay(durationToSleep, stoppingToken);
                        continue;
                    }

                    // Record that we've just started another loop
                    if (lastExecution == default)
                    {
                        lastExecution = new Models.SynchronizedBackgroundService { Name = this.GetType().FullName };
                        _ = nsContext.SynchronizedBackgroundServices.Add(lastExecution);
                    }
                    lastExecution.LastExecuted = DateTimeOffset.UtcNow;
                    lastExecution.CountExecutions++;
                    _ = await nsContext.SaveChangesAsync(stoppingToken);
                }

                // And then actually do the work
                using IOperationHolder<RequestTelemetry> op = this.telemetryClient.StartOperation<RequestTelemetry>(this.GetType().FullName);
                this.op = op;
                try
                {
                    using CancellationTokenSource intervalTokenSource = new(this.Interval);
                    using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, intervalTokenSource.Token);
                    await this.ExecuteOnceAsync(linkedTokenSource.Token);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    this.logger.LogError(ex, "Failed execution.");
                }
            }
        }

        protected abstract TimeSpan Interval { get; }

        protected abstract Task ExecuteOnceAsync(CancellationToken cancellationToken);
    }
}
