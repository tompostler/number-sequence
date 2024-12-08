using System.CommandLine;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class PingCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command command = new("ping", "Ping the service to verify configuration and availability.");
            Option<bool> pingAuthOption = new("--authed", "If requested, make the ping request with authentication.");
            command.AddOption(pingAuthOption);
            Option<bool> pingAuthRoleOption = new("--roled", "If requested, make the ping request with authentication to an endpoint that requies a role.");
            command.AddOption(pingAuthRoleOption);
            command.SetHandler(HandleAsync, pingAuthOption, pingAuthRoleOption, stampOption, verbosityOption);
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
