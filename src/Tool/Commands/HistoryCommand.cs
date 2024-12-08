using System.CommandLine;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class HistoryCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command command = new("history", "Get the last 25 commit messages.");

            Option<bool> remoteOption = new("--remote", "Get the most recent 25 commits from the server instead of built-in to the tool.");
            command.AddOption(remoteOption);

            command.SetHandler(HandleAsync, remoteOption, stampOption, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(bool remote, Stamp stamp, Verbosity verbosity)
        {
            if (remote)
            {
                NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, stamp);
                Console.WriteLine(await client.History.GetAsync());
            }
            else
            {
                Console.WriteLine(Contracts.History.Commits);
            }
        }
    }
}
