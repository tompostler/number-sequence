using System.CommandLine;
using System.Threading.Tasks;
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
            rootCommand.AddCommand(LatexStatusCommand.Create(verbosityOption));
            rootCommand.AddCommand(PingCommand.Create(verbosityOption));
            rootCommand.AddCommand(RandomCommand.Create(verbosityOption));
            rootCommand.AddCommand(TokenCommand.Create(verbosityOption));

            return await rootCommand.InvokeAsync(args);
        }
    }
}
