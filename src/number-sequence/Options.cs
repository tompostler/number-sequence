using Unlimitedinf.Utilities.Extensions;

namespace number_sequence
{
    public static class Options
    {
        public static IServiceCollection AddNsConfig(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services.Configure<Claude>(configuration.GetSection(nameof(Claude)));
            _ = services.Configure<Email>(configuration.GetSection(nameof(Email)));
            _ = services.Configure<Google>(configuration.GetSection(nameof(Google)));
            _ = services.Configure<Sql>(configuration.GetSection(nameof(Sql)));
            _ = services.Configure<Storage>(configuration.GetSection(nameof(Storage)));
            return services;
        }

        /// <summary>
        /// Named after the model rather than the vendor so that it does not collide with the <c>Anthropic</c> sdk namespace at every call site that needs both.
        /// </summary>
        public sealed class Claude
        {
            public string ApiKey { get; set; }
        }

        public sealed class Email
        {
            public string ChiroBatchMap { get; set; }
            public Dictionary<string, ChiroClinic> ChiroBatchMapParsed => this.ChiroBatchMap.FromJsonString<Dictionary<string, ChiroClinic>>();
            public string ChiroBatchUri { get; set; }
            public string Server { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string LocalDevToOverride { get; set; }
        }

        public sealed class ChiroClinic
        {
            /// <summary>Spelled out clinic name for dictation parsing assistance.</summary>
            public string Name { get; set; }
            public string Email { get; set; }
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
