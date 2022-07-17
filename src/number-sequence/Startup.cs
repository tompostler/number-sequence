using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            _ = services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
                );

            _ = services.AddLogging(options => options.AddConsole());
            _ = services.AddApplicationInsightsTelemetry();

            _ = services.AddNsConfig(this.Configuration);

            _ = services.AddSingleton<DataAccess.AccountDataAccess>();
            _ = services.AddSingleton<DataAccess.CountDataAccess>();
            _ = services.AddSingleton<DataAccess.TokenDataAccess>();
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
