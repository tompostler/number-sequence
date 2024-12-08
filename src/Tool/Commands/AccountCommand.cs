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

            Command createCommand = new("create", "Create a new account.");
            createCommand.SetHandler(HandleCreateAsync, stampOption, verbosityOption);

            Command getCommand = new("get", "Get an existing account to see its properties.");
            getCommand.AddAlias("show");
            Argument<string> accountNameArgument = new("name", "The name of the account.");
            getCommand.AddArgument(accountNameArgument);
            getCommand.SetHandler(HandleGetAsync, accountNameArgument, stampOption, verbosityOption);

            rootCommand.AddCommand(createCommand);
            rootCommand.AddCommand(getCommand);
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
