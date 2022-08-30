using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Tool.Commands;

namespace TcpWtf.NumberSequence.Tool
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand = new("A CLI for ns.tcp.wtf.");
            Option<LogLevel> verbosityOption = new("--verbosity", () => LogLevel.Warning, "Verbosity level for logging.");
            verbosityOption.AddAlias("-v");
            rootCommand.AddGlobalOption(verbosityOption);

            rootCommand.AddCommand(PingCommand.Create(verbosityOption));
            rootCommand.AddCommand(RandomCommand.Create(verbosityOption));

            try
            {
                return await rootCommand.InvokeAsync(args);
            }
            finally
            {
                // Need to give the logger an opportunity to flush to the console
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
