using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using number_sequence.DataAccess;
using number_sequence.Middleware;
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
            services.AddMemoryCache(options =>
            {
                options.CompactionPercentage = .25;
                options.SizeLimit = 1024;
            });
            services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
                );
            services.AddLogging(options => options.AddConsole());
            services.AddApplicationInsightsTelemetry();
            services.AddSingleton<AccountDataAccess>();
            services.AddSingleton<CountDataAccess>();
            services.AddSingleton<TokenDataAccess>();
            services.Configure<Options.CosmosDB>(this.Configuration.GetSection(nameof(Options.CosmosDB)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseMiddleware<ExceptionToStatusCodeMiddleware>();

            var assemblyFileVersion = FileVersionInfo.GetVersionInfo(typeof(Startup).Assembly.Location).ProductVersion;
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("ns-tcp-wtf-server-version", assemblyFileVersion);
                await next.Invoke();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
