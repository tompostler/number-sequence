using System.CommandLine;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class IpCommand
    {
        public static Command Create(Option<Verbosity> verbosityOption)
        {
            Command command = new("ip", "Ping the service and display the ip address the server saw the request originate from.");
            command.SetHandler(HandleAsync, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, Program.Stamp);
            Console.WriteLine(await client.Ping.GetPublicIpAsync());
        }
    }
}
