using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class AccountCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("account", "Create or get an account.");

            Command createCommand = new("create", "Create a new account.")
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

            Argument<string> accountNameArgument = new("name") { Description = "The name of the account." };
            Command getCommand = new("get", "Get an existing account to see its properties.")
            {
                stampOption,
                verbosityOption,
                accountNameArgument,
            };
            getCommand.Aliases.Add("show");
            getCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    string name = parseResult.GetRequiredValue(accountNameArgument);
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleGetAsync(name, stamp, verbosity);
                });

            rootCommand.Subcommands.Add(createCommand);
            rootCommand.Subcommands.Add(getCommand);
            return rootCommand;
        }

        private static async Task HandleCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, stamp);

            Contracts.Account account = new()
            {
                Name = Input.GetString(nameof(account.Name)),
                Key = Input.GetString(nameof(account.Key)),
            };

            account = await client.Account.CreateAsync(account);
            Console.WriteLine(account.ToJsonString(indented: true));
        }

        private static async Task HandleGetAsync(string name, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, stamp);
            Contracts.Account account = await client.Account.GetAsync(name);
            Console.WriteLine(account.ToJsonString(indented: true));
        }
    }
}
