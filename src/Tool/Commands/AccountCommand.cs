using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class AccountCommand
    {
        public static Command Create(Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("account", "Create or get an account.");

            Command createCommand = new("create", "Create a new account.");
            createCommand.SetHandler(HandleCreateAsync, verbosityOption);

            Command getCommand = new("get", "Get an existing account to see its properties.");
            getCommand.AddAlias("show");
            Argument<string> accountNameArgument = new("name", "The name of the account.");
            getCommand.AddArgument(accountNameArgument);
            getCommand.SetHandler(HandleGetAsync, accountNameArgument, verbosityOption);

            rootCommand.AddCommand(createCommand);
            rootCommand.AddCommand(getCommand);
            return rootCommand;
        }

        private static async Task HandleCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, Program.Stamp);

            Contracts.Account account = new()
            {
                Name = Input.GetString(nameof(account.Name)),
                Key = Input.GetString(nameof(account.Key)),
            };

            account = await client.Account.CreateAsync(account);
            Console.WriteLine(account.ToJsonString(indented: true));
        }

        private static async Task HandleGetAsync(string name, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, Program.Stamp);
            Contracts.Account account = await client.Account.GetAsync(name);
            Console.WriteLine(account.ToJsonString(indented: true));
        }
    }
}
