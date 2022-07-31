using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace number_sequence
{
    public static class Options
    {
        public static IServiceCollection AddNsConfig(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services.Configure<CosmosDB>(configuration.GetSection(nameof(CosmosDB)));
            _ = services.Configure<Email>(configuration.GetSection(nameof(Email)));
            _ = services.Configure<Google>(configuration.GetSection(nameof(Google)));
            _ = services.Configure<Sql>(configuration.GetSection(nameof(Sql)));
            _ = services.Configure<Storage>(configuration.GetSection(nameof(Storage)));
            return services;
        }

        public sealed class CosmosDB
        {
            public string Endpoint { get; set; }
            public string Key { get; set; }
            public string DatabaseId { get; set; }
            public string ContainerId { get; set; }
        }

        public sealed class Email
        {
            public string Server { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public sealed class Google
        {
            public string Credentials { get; set; }
        }

        public sealed class Sql
        {
            public string ConnectionString { get; set; }
        }

        public sealed class Storage
        {
            public string ConnectionString { get; set; }
        }
    }
}
