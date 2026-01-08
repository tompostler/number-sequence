using System.CommandLine;
using System.Text.Json.Nodes;
using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class TokenCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("token", "Create, save, or inspect an existing token.");

            Command createCommand = new("create", "Create a new token for your account.")
            {
                stampOption,
                verbosityOption,
            };
            createCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleCreateAsync(stamp, verbosity);
                });

            Argument<string> nameArgument = new("tokenName") { Description = "The name of the token." };
            Command deleteCommand = new("delete", "Delete an existing token for your account.")
            {
                stampOption,
                verbosityOption,
                nameArgument,
            };
            deleteCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string name = parseResult.GetRequiredValue(nameArgument);
                    return HandleDeleteAsync(name, stamp, verbosity);
                });

            Command inspectCommand = new("inspect", "Decrypt an existing token to see its properties.");
            inspectCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    HandleInspect();
                    return Task.CompletedTask;
                });

            Command readCommand = new("read", "Inspects the token on disk.")
            {
                verbosityOption,
            };
            readCommand.Aliases.Add("show");
            readCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    HandleRead(verbosity);
                    return Task.CompletedTask;
                });

            Command saveCommand = new("save", "Saves the token to disk for usage in authenticated commands.")
            {
                verbosityOption,
            };
            saveCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    HandleSave(verbosity);
                    return Task.CompletedTask;
                });

            rootCommand.Subcommands.Add(createCommand);
            rootCommand.Subcommands.Add(deleteCommand);
            rootCommand.Subcommands.Add(inspectCommand);
            rootCommand.Subcommands.Add(saveCommand);
            rootCommand.Subcommands.Add(readCommand);
            return rootCommand;
        }

        private static async Task HandleCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, stamp);

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

        private static async Task HandleDeleteAsync(string name, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Token token = await client.Token.DeleteAsync(name);
            Console.WriteLine(token.ToJsonString(indented: true));
        }

        private static void HandleInspect() => Console.WriteLine(Input.GetString("Token").FromBase64JsonString<JsonObject>().ToString());

        private static void HandleRead(Verbosity verbosity) => Console.WriteLine(TokenProvider.Get(new Logger(verbosity)).FromBase64JsonString<JsonObject>().ToString());

        private static void HandleSave(Verbosity verbosity)
        {
            var logger = new Logger(verbosity);
            TokenProvider.Upsert(Input.GetString("Token"), logger);
            Console.WriteLine("Saved token to disk.");
        }
    }
}
