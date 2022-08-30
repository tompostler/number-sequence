using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class PingCommand
    {
        public static Command Create(Option<LogLevel> verbosityOption)
        {
            Command command = new("ping", "Ping the service to verify configuration and availability.");
            Option<bool> pingAuthOption = new("--authed", "If requested, make the ping request with authentication.");
            command.AddOption(pingAuthOption);
            command.SetHandler(HandleAsync, pingAuthOption, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(bool authed, LogLevel logLevel)
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
    }
}
