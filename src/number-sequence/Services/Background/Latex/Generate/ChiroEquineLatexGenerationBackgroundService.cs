using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using number_sequence.DataAccess;
using number_sequence.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace number_sequence.Services.Background.Latex.Generate
{
    public sealed class ChiroEquineLatexGenerationBackgroundService : SqlSynchronizedBackgroundService
    {
        private readonly GoogleSheetDataAccess googleSheetDataAccess;
        private readonly NsStorage nsStorage;

        private readonly string googleSheetId;
        private readonly string googleSheetRange;

        public ChiroEquineLatexGenerationBackgroundService(
            GoogleSheetDataAccess googleSheetDataAccess,
            NsStorage nsStorage,
            IOptions<Options.Google> googleOptions,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ChiroEquineLatexGenerationBackgroundService> logger,
            TelemetryClient telemetryClient)
            : base(serviceProvider, sentinals, logger, telemetryClient)
        {
            this.googleSheetDataAccess = googleSheetDataAccess;
            this.nsStorage = nsStorage;

            this.googleSheetId = googleOptions.Value.SheetChiroEquineId;
            this.googleSheetRange = googleOptions.Value.SheetChiroEquineRange;
        }

        protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

        protected override async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            IList<IList<object>> data = await this.googleSheetDataAccess.GetAsync(this.googleSheetId, this.googleSheetRange, cancellationToken);

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
        }
    }
}
