using System.CommandLine;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class HistoryCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Option<bool> remoteOption = new("--remote") { Description = "Get the most recent 25 commits from the server instead of built-in to the tool." };
            
            Command command = new("history", "Get the last 25 commit messages.")
            {
                stampOption,
                verbosityOption,
                remoteOption,
            };
            command.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    bool remote = parseResult.GetValue(remoteOption);
                    return HandleAsync(remote, stamp, verbosity);
                });

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
