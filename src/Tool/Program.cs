using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand = new("A CLI for ns.tcp.wtf.");
            Option<LogLevel> verbosityOption = new("--verbosity", () => LogLevel.Warning);
            verbosityOption.AddAlias("-v");
            rootCommand.AddGlobalOption(verbosityOption);
            rootCommand.SetHandler(() => Console.WriteLine("Hello world!"));

            Command pingCommand = new("ping", "Ping the service to verify configuration and availability.");
            Option<bool> pingAuthOption = new("--authed", "If requested, make the ping request with authentication.");
            pingCommand.AddOption(pingAuthOption);
            pingCommand.SetHandler(PingAsync, pingAuthOption, verbosityOption);
            rootCommand.AddCommand(pingCommand);

            Command randomCommand = new("random", "Get random data.");
            Argument<string> randomTypeArg = new Argument<string>("type", "The type of random to get. Pick from the supported values.")
                .FromAmong(
                    "8ball",
                    "guid",
                    "name",
                    "bit",
                    "crumb",
                    "nibble",
                    "byte",
                    "short",
                    "int",
                    "long"
                );
            randomCommand.AddArgument(randomTypeArg);
            randomCommand.SetHandler(RandomAsync, randomTypeArg, verbosityOption);
            rootCommand.AddCommand(randomCommand);

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

        private static async Task PingAsync(bool authed, LogLevel logLevel)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(logLevel).AddConsole());
            NsTcpWtfClient client = new(loggerFactory.CreateLogger<NsTcpWtfClient>(), (_) => Task.FromResult(string.Empty));

            if (authed)
            {
                throw new NotImplementedException();
            }
            else
            {
                await client.Ping.SendAsync();
            }
        }

        private static async Task RandomAsync(string type, LogLevel logLevel)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(logLevel).AddConsole());
            NsTcpWtfClient client = new(loggerFactory.CreateLogger<NsTcpWtfClient>(), (_) => Task.FromResult(string.Empty));

            object response = type switch
            {
                "8ball" => await client.Random.Get8BallAsync(),
                "guid" => await client.Random.GetGuidAsync(),
                "name" => await client.Random.GetNameAsync(),
                "bit" => await client.Random.GetULong01Async(),
                "crumb" => await client.Random.GetULong02Async(),
                "nibble" => await client.Random.GetULong04Async(),
                "byte" => await client.Random.GetULong08Async(),
                "short" => await client.Random.GetULong16Async(),
                "int" => await client.Random.GetULong32Async(),
                "long" => await client.Random.GetULong64Async(),
                _ => throw new NotImplementedException($"{type} was not implemented.")
            };
            Console.WriteLine(response);
        }
    }
}
