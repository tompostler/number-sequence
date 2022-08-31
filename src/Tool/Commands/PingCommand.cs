using System.CommandLine;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class PingCommand
    {
        public static Command Create(Option<Verbosity> verbosityOption)
        {
            Command command = new("ping", "Ping the service to verify configuration and availability.");
            Option<bool> pingAuthOption = new("--authed", "If requested, make the ping request with authentication.");
            command.AddOption(pingAuthOption);
            command.SetHandler(HandleAsync, pingAuthOption, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(bool authed, Verbosity verbosity)
        {
            if (authed)
            {
                NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync);
                await client.Ping.SendWithAuthAsync();
            }
            else
            {
                NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync);
                await client.Ping.SendAsync();
            }
        }
    }
}
