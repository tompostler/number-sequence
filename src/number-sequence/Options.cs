using Unlimitedinf.Utilities.Extensions;

namespace number_sequence
{
    public static class Options
    {
        public static IServiceCollection AddNsConfig(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services.Configure<Email>(configuration.GetSection(nameof(Email)));
            _ = services.Configure<Google>(configuration.GetSection(nameof(Google)));
            _ = services.Configure<Sql>(configuration.GetSection(nameof(Sql)));
            _ = services.Configure<Storage>(configuration.GetSection(nameof(Storage)));
            return services;
        }

        public sealed class Email
        {
            public string ChiroBatchMap { get; set; }
            public Dictionary<string, string> ChiroBatchMapParsed => this.ChiroBatchMap.FromJsonString<Dictionary<string, string>>();
            public string ChiroBatchUri { get; set; }
            public string Server { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string LocalDevToOverride { get; set; }
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
