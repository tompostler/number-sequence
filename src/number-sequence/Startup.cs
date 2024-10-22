using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using number_sequence.Services.Background.LatexGeneration;
using System.Text.Json.Serialization;
using TcpWtf.NumberSequence.Contracts.Framework;

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
            _ = services
                .AddControllers(options =>
                {
                    options.RespectBrowserAcceptHeader = true;
                    options.OutputFormatters.Add(new Formatters.DaysSinceEventsTextHtmlOutputFormatter());
                    options.OutputFormatters.Add(new Formatters.DaysSinceTextHtmlOutputFormatter());
                })
                .AddJsonOptions(options =>
                {
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

            _ = services.AddHostedService<ChiroCanineLatexGenerationBackgroundService>();
            _ = services.AddHostedService<ChiroEquineLatexGenerationBackgroundService>();
            _ = services.AddHostedService<ReprocessInvoiceRegularlyBackgroundService>();


            //
            // Durable orchestrations
            //

            _ = DurableTaskImpl.ServiceProviderOrchestrationExtensions.AddDurableOrchestrations(services);
            _ = services.AddHostedService<Services.Background.DurableOrchestrationWorkerBackgroundService>();
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

            // Custom versioning middleware
            string assemblyFileVersion = ThisAssembly.AssemblyInformationalVersion;
            _ = app.Use(
                async (context, next) =>
                {
                    context.Response.Headers[HttpHeaderNames.ServerVersion] = assemblyFileVersion;
                    await next.Invoke();
                });

            _ = app.UseEndpoints(endpoints => _ = endpoints.MapControllers());
        }
    }
}
