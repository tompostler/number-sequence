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

            Option<Stamp> stampOption = new("--stamp")
            {
                Description = "Stamp to use for the client.",
                DefaultValueFactory = _ => Stamp.Public,
            };
            Option<Verbosity> verbosityOption = new("--verbosity", "-v")
            {
                Description = "Verbosity level for logging.",
                DefaultValueFactory = _ => Verbosity.Warn,
            };

            rootCommand.Subcommands.Add(AccountCommand.Create(stampOption, verbosityOption));
            rootCommand.Subcommands.Add(CountCommand.Create(stampOption, verbosityOption));
            rootCommand.Subcommands.Add(DailySequenceValueCommand.Create(stampOption, verbosityOption));
            rootCommand.Subcommands.Add(DailySequenceValueConfigCommand.Create(stampOption, verbosityOption));
            rootCommand.Subcommands.Add(DaysSinceCommand.Create(stampOption, verbosityOption));
            rootCommand.Subcommands.Add(HistoryCommand.Create(stampOption, verbosityOption));
            rootCommand.Subcommands.Add(IpCommand.Create(stampOption, verbosityOption));
            rootCommand.Subcommands.Add(PdfStatusCommand.Create(stampOption, verbosityOption));
            rootCommand.Subcommands.Add(PingCommand.Create(stampOption, verbosityOption));
            rootCommand.Subcommands.Add(RandomCommand.Create(stampOption, verbosityOption));
            rootCommand.Subcommands.Add(RedirectCommand.Create(stampOption, verbosityOption));
            rootCommand.Subcommands.Add(TokenCommand.Create(stampOption, verbosityOption));

            ParseResult parseResult = rootCommand.Parse(args);
            return await parseResult.InvokeAsync();
        }
    }
}
