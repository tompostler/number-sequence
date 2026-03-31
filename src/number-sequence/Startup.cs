using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuestPDF.Drawing;
using System.Text.Json.Serialization;
using TcpWtf.NumberSequence.Client;

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
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            _ = services.AddRazorPages();
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

            // Singleton so the underlying HttpClient is reused across requests (avoids socket exhaustion).
            // The token callback captures IHttpContextAccessor and reads the user lazily at call time.
            // NOTE: The factory resolves IServerAddressesFeature to get the bound address. This works because the
            //       singleton is first resolved on an HTTP request, after the server has finished binding. If this
            //       client is ever injected into a hosted service that runs before the first request, Addresses may
            //       still be empty and the factory will throw.
            _ = services.AddSingleton(sp =>
            {
                IServerAddressesFeature addresses = sp.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
                var listenUri = new Uri(addresses.Addresses.First());
                // The server binds to an unspecified address (0.0.0.0 or [::]) which can't be used
                // as an outgoing connection target. Always construct the loopback address explicitly
                // using just the bound port so IPv6/IPv4 wildcard ambiguity doesn't matter.
                var baseAddress = new Uri($"http://localhost:{listenUri.Port}");
                IHttpContextAccessor contextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                return new NsTcpWtfClient(
                    sp.GetRequiredService<ILogger<NsTcpWtfClient>>(),
                    (_, _) => Task.FromResult((contextAccessor.HttpContext?.User as Filters.RequiresTokenFilter.TokenPrincipal)?.RawToken),
                    baseAddress);
            });

            _ = services.AddHttpContextAccessor();
            _ = services.AddScoped<Services.TokenValidationService>();

            _ = services.AddSingleton<Utilities.Delays>();
            _ = services.AddSingleton<Utilities.Sentinals>();


            //
            // Background services
            //

            _ = services.AddHostedService<Services.Background.GoogleSheetPdfGeneration.ChiroCanineGoogleSheetPdfGenerationBackgroundService>();
            _ = services.AddHostedService<Services.Background.GoogleSheetPdfGeneration.ChiroEquineGoogleSheetPdfGenerationBackgroundService>();
            _ = services.AddHostedService<Services.Background.ChiroBatchSendBackgroundService>();
            _ = services.AddHostedService<Services.Background.ReprocessLedgerRegularlyBackgroundService>();


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
            _ = app.UseMiddleware<Middleware.TokenCookieMiddleware>();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllers();
                _ = endpoints.MapRazorPages();
            });
        }
    }
}
