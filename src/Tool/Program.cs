using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Tool.Commands;

namespace TcpWtf.NumberSequence.Tool
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand = new("A CLI for ns.tcp.wtf.");

            Option<Stamp> stampOption = new("--stamp", () => Stamp.Public, "Stamp to use for the client.");
            rootCommand.AddGlobalOption(stampOption);

            Option<Verbosity> verbosityOption = new("--verbosity", () => Verbosity.Warn, "Verbosity level for logging.");
            verbosityOption.AddAlias("-v");
            rootCommand.AddGlobalOption(verbosityOption);

            rootCommand.AddCommand(AccountCommand.Create(stampOption, verbosityOption));
            rootCommand.AddCommand(CountCommand.Create(stampOption, verbosityOption));
            rootCommand.AddCommand(DailySequenceValueCommand.Create(stampOption, verbosityOption));
            rootCommand.AddCommand(DaysSinceCommand.Create(stampOption, verbosityOption));
            rootCommand.AddCommand(HistoryCommand.Create(stampOption, verbosityOption));
            rootCommand.AddCommand(IpCommand.Create(stampOption, verbosityOption));
            rootCommand.AddCommand(PdfStatusCommand.Create(stampOption, verbosityOption));
            rootCommand.AddCommand(PingCommand.Create(stampOption, verbosityOption));
            rootCommand.AddCommand(RandomCommand.Create(stampOption, verbosityOption));
            rootCommand.AddCommand(RedirectCommand.Create(stampOption, verbosityOption));
            rootCommand.AddCommand(TokenCommand.Create(stampOption, verbosityOption));

            return await rootCommand.InvokeAsync(args);
        }
    }
}
