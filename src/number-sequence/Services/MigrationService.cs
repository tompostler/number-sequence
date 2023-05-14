using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Utilities;

namespace number_sequence.Services
{
    public sealed class MigrationService : IHostedService
    {
        private readonly Sentinals sentinals;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<MigrationService> logger;

        public MigrationService(
            Sentinals sentinals,
            IServiceProvider serviceProvider,
            ILogger<MigrationService> logger)
        {
            this.sentinals = sentinals;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Applying migrations if necessary...");

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            await nsContext.Database.MigrateAsync(cancellationToken);

            this.logger.LogInformation("Migrations complete.");
            this.sentinals.DBMigration.SignalCompletion();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
