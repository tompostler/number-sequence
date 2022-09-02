using System;
using System.CommandLine;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Tool.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class LatexStatusCommand
    {
        public static Command Create(Option<Verbosity> verbosityOption)
        {
            Command command = new("latex-status", "Get the status of the latex background services.");
            command.SetHandler(HandleAsync, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            LatexStatus latexStatus = await client.LatexStatus.GetAsync();
            Console.WriteLine(latexStatus.ToJsonString());
        }
    }
}
