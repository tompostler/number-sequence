using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Tool.Commands;

namespace TcpWtf.NumberSequence.Tool
{
    internal static class Program
    {
        internal static Stamp Stamp = Stamp.Public;

        public static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand = new("A CLI for ns.tcp.wtf.");
            Option<Verbosity> verbosityOption = new("--verbosity", () => Verbosity.Warn, "Verbosity level for logging.");
            verbosityOption.AddAlias("-v");
            rootCommand.AddGlobalOption(verbosityOption);

            rootCommand.AddCommand(AccountCommand.Create(verbosityOption));
            rootCommand.AddCommand(CountCommand.Create(verbosityOption));
            rootCommand.AddCommand(InvoiceCommand.Create(verbosityOption));
            rootCommand.AddCommand(IpCommand.Create(verbosityOption));
            rootCommand.AddCommand(LatexStatusCommand.Create(verbosityOption));
            rootCommand.AddCommand(PingCommand.Create(verbosityOption));
            rootCommand.AddCommand(RandomCommand.Create(verbosityOption));
            rootCommand.AddCommand(TokenCommand.Create(verbosityOption));

            Option<bool> historyOption = new("--history", "Display version history of the last 25 commits.");
            rootCommand.AddOption(historyOption);
            rootCommand.SetHandler(Handle, historyOption, verbosityOption);

            return await rootCommand.InvokeAsync(args);
        }

        private static void Handle(bool history, Verbosity verbosity)
        {
            if (history)
            {
                Console.WriteLine("""
HISTORY_PLACEHOLDER
""");
            }
        }
    }
}
