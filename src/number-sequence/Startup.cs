using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using System.Diagnostics;
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
            _ = services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
                );

            _ = services.AddLogging(options => options.AddConsole().AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information));
            _ = services.AddApplicationInsightsTelemetry();

            _ = services.AddNsConfig(this.Configuration);


            //
            // Data Access
            //

            // Cosmos DB
            _ = services.AddSingleton<DataAccess.AccountDataAccess>();
            _ = services.AddSingleton<DataAccess.CountDataAccess>();
            _ = services.AddSingleton<DataAccess.TokenDataAccess>();
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

            _ = services.AddSingleton<Utilities.Sentinals>();


            //
            // Background services
            //

            _ = services.AddHostedService<Services.Background.LatexGenerationBackgroundService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            _ = app.UseDeveloperExceptionPage();
            _ = app.UseHttpsRedirection();
            _ = app.UseRouting();

            _ = app.UseMiddleware<Middleware.ExceptionToStatusCodeMiddleware>();

            string assemblyFileVersion = FileVersionInfo.GetVersionInfo(typeof(Startup).Assembly.Location).ProductVersion;
            _ = app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("ns-tcp-wtf-server-version", assemblyFileVersion);
                await next.Invoke();
            });

            _ = app.UseEndpoints(endpoints => _ = endpoints.MapControllers());
        }
    }
}
