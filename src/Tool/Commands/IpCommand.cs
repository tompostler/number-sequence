using System.CommandLine;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class IpCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command command = new("ip", "Ping the service and display the ip address the server saw the request originate from.");
            command.SetHandler(HandleAsync, stampOption, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, stamp);
            Console.WriteLine(await client.Ping.GetPublicIpAsync());
        }
    }
}
