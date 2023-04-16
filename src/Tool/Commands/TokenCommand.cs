using System;
using System.CommandLine;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class TokenCommand
    {
        public static Command Create(Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("token", "Create, save, or inspect an existing token.");

            Argument<string> tokenArgument = new("tokenValue", "The actual token.");

            Command createCommand = new("create", "Create a new token for your account.");
            createCommand.SetHandler(HandleCreateAsync, verbosityOption);

            Command inspectCommand = new("inspect", "Decrypt an existing token to see its properties.");
            inspectCommand.AddArgument(tokenArgument);
            inspectCommand.SetHandler(HandleInspect, tokenArgument);

            Command readCommand = new("read", "Inspects the token on disk.");
            readCommand.AddAlias("show");
            readCommand.SetHandler(HandleRead, verbosityOption);

            Command saveCommand = new("save", "Saves the token to disk for usage in authenticated commands.");
            saveCommand.AddArgument(tokenArgument);
            saveCommand.SetHandler(HandleSave, tokenArgument, verbosityOption);

            rootCommand.AddCommand(createCommand);
            rootCommand.AddCommand(inspectCommand);
            rootCommand.AddCommand(saveCommand);
            rootCommand.AddCommand(readCommand);
            return rootCommand;
        }

        private static async Task HandleCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, Program.Stamp);

            Contracts.Token token = new()
            {
                Account = Input.GetString(nameof(token.Account)),
                Key = Input.GetString(nameof(token.Key)),
                Name = Input.GetString(nameof(token.Name)),
                ExpirationDate = Input.GetDateTime(nameof(token.ExpirationDate))
            };

            token = await client.Token.CreateAsync(token);
            Console.WriteLine(token.ToJsonString(indented: true));
        }

        private static void HandleInspect(string token) => Console.WriteLine(token.FromBase64JsonString<JsonObject>().ToString());

        private static void HandleRead(Verbosity verbosity) => Console.WriteLine(TokenProvider.Get(new Logger(verbosity)).FromBase64JsonString<JsonObject>().ToString());

        private static void HandleSave(string token, Verbosity verbosity)
        {
            var logger = new Logger(verbosity);
            TokenProvider.Upsert(token, logger);
            Console.WriteLine("Saved token to disk.");
        }
    }
}
