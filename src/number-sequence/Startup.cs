using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using QuestPDF.Drawing;
using System.Text.Json.Serialization;

namespace number_sequence
{
    public sealed class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddMemoryCache();
            _ = services.AddHttpClient();
            _ = services
                .AddControllers(options =>
                {
                    options.RespectBrowserAcceptHeader = true;
                    options.OutputFormatters.Add(new Formatters.DaysSinceEventsTextHtmlOutputFormatter());
                    options.OutputFormatters.Add(new Formatters.DaysSinceTextHtmlOutputFormatter());
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            _ = services.AddLogging(options => options.AddConsole().AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information));
            _ = services.AddApplicationInsightsTelemetry();

            _ = services.AddNsConfig(this.Configuration);


            //
            // Data Access
            //

            // Email
            _ = services.AddSingleton<DataAccess.EmailDataAccess>();
            // Google
            _ = services.AddSingleton<DataAccess.GoogleSheetDataAccess>();
            // SQL
            _ = services.AddDbContext<DataAccess.NsContext>((provider, options) => options
                  .UseSqlServer(
                      provider.GetRequiredService<IOptions<Options.Sql>>().Value.ConnectionString,
                      sqloptions => sqloptions
                          .EnableRetryOnFailure()));
            _ = services.AddHostedService<Services.MigrationService>();
            // Storage Account
            _ = services.AddSingleton<DataAccess.NsStorage>();


            //
            // Utilities
            //

            _ = services.AddSingleton<Utilities.Delays>();
            _ = services.AddSingleton<Utilities.Sentinals>();


            //
            // Background services
            //

            _ = services.AddHostedService<Services.Background.GoogleSheetPdfGeneration.ChiroCanineGoogleSheetPdfGenerationBackgroundService>();
            _ = services.AddHostedService<Services.Background.GoogleSheetPdfGeneration.ChiroEquineGoogleSheetPdfGenerationBackgroundService>();
            _ = services.AddHostedService<Services.Background.ChiroBatchSendBackgroundService>();


            //
            // Durable orchestrations
            //

            _ = DurableTaskImpl.ServiceProviderOrchestrationExtensions.AddDurableOrchestrations(services);
            _ = services.AddHostedService<Services.Background.DurableOrchestrationWorkerBackgroundService>();


            //
            // Misc
            //

            FontManager.RegisterFont(Resources.ComputerModernRomanFont);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            _ = app.UseForwardedHeaders(
                new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.All
                });

            _ = app.UseDeveloperExceptionPage();
            _ = app.UseHttpsRedirection();
            _ = app.UseRouting();

            _ = app.UseMiddleware<Middleware.ExceptionToStatusCodeMiddleware>();
            _ = app.UseMiddleware<Middleware.ServerMetadataMiddleware>();

            _ = app.UseEndpoints(endpoints => _ = endpoints.MapControllers());
        }
    }
}
