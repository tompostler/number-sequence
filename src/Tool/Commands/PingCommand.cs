using System.CommandLine;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class PingCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Option<bool> pingAuthOption = new("--authed") { Description = "If requested, make the ping request with authentication." };
            Option<bool> pingAuthRoleOption = new("--roled") { Description = "If requested, make the ping request with authentication to an endpoint that requies a role." };
            Command command = new("ping", "Ping the service to verify configuration and availability.")
            {
                stampOption,
                verbosityOption,
                pingAuthOption,
                pingAuthRoleOption,
            };
            command.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    bool pingAuth = parseResult.GetValue(pingAuthOption);
                    bool pingAuthRoled = parseResult.GetValue(pingAuthRoleOption);
                    return HandleAsync(pingAuth, pingAuthRoled, stamp, verbosity);
                });
            return command;
        }

        private static async Task HandleAsync(bool authed, bool roled, Stamp stamp, Verbosity verbosity)
        {
            if (authed || roled)
            {
                NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
                if (roled)
                {
                    await client.Ping.SendWithAuthToRoleAsync();
                }
                else
                {
                    await client.Ping.SendWithAuthAsync();
                }
            }
            else
            {
                NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, stamp);
                await client.Ping.SendAsync();
            }
        }
    }
}
