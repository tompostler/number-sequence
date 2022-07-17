using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace number_sequence
{
    public static class Options
    {
        public static IServiceCollection AddNsConfig(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services.Configure<CosmosDB>(configuration.GetSection(nameof(CosmosDB)));
            _ = services.Configure<Google>(configuration.GetSection(nameof(Google)));
            _ = services.Configure<Sql>(configuration.GetSection(nameof(Sql)));
            return services;
        }

        public sealed class CosmosDB
        {
            public string Endpoint { get; set; }
            public string Key { get; set; }
            public string DatabaseId { get; set; }
            public string ContainerId { get; set; }
        }

        public sealed class Google
        {
            public string Credentials { get; set; }
        }

        public sealed class Sql
        {
            public string ConnectionString { get; set; }
        }
    }
}
