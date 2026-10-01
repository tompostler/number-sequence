using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class AccountCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("account", "Create or get an account, or administer accounts with the admin role.");

            Command createCommand = new("create", "Create a new account.")
            {
                stampOption,
                verbosityOption,
            };
            createCommand.AddCreateAliases();
            createCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleCreateAsync(stamp, verbosity);
                });

            Argument<string> accountNameArgument = new("name") { Description = "The name of the account." };
            Command readCommand = new("read", "Read an existing account to see its properties.")
            {
                stampOption,
                verbosityOption,
                accountNameArgument,
            };
            readCommand.AddReadAliases();
            readCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    string name = parseResult.GetRequiredValue(accountNameArgument);
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleGetAsync(name, stamp, verbosity);
                });

            Command listCommand = new("list", "List all accounts. Requires the admin role.")
            {
                stampOption,
                verbosityOption,
            };
            listCommand.AddListAliases();
            listCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleListAsync(stamp, verbosity);
                });

            Argument<AccountTier> tierArgument = new("tier") { Description = "The tier to set." };
            Command tierCommand = new("tier", "Set the tier of an account. Requires the admin role.")
            {
                stampOption,
                verbosityOption,
                accountNameArgument,
                tierArgument,
            };
            tierCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    string name = parseResult.GetRequiredValue(accountNameArgument);
                    AccountTier tier = parseResult.GetRequiredValue(tierArgument);
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleTierAsync(name, tier, stamp, verbosity);
                });

            Argument<string> knownRoleArgument = new Argument<string>("role") { Description = "The role to grant." }.AcceptOnlyFromAmong([.. AccountRoles.All]);
            Command roleAddCommand = new("add", "Grant a role to an account.")
            {
                stampOption,
                verbosityOption,
                accountNameArgument,
                knownRoleArgument,
            };
            roleAddCommand.AddCreateAliases();
            roleAddCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    string name = parseResult.GetRequiredValue(accountNameArgument);
                    string role = parseResult.GetRequiredValue(knownRoleArgument);
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleRoleAddAsync(name, role, stamp, verbosity);
                });

            // Not restricted to known roles so stale values written by hand can be cleaned up.
            Argument<string> anyRoleArgument = new("role") { Description = "The role to remove." };
            Command roleRemoveCommand = new("remove", "Remove a role from an account.")
            {
                stampOption,
                verbosityOption,
                accountNameArgument,
                anyRoleArgument,
            };
            roleRemoveCommand.AddDeleteAliases();
            roleRemoveCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    string name = parseResult.GetRequiredValue(accountNameArgument);
                    string role = parseResult.GetRequiredValue(anyRoleArgument);
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleRoleRemoveAsync(name, role, stamp, verbosity);
                });

            Command roleCommand = new("role", "Grant or remove account roles. Requires the admin role.");
            roleCommand.Subcommands.Add(roleAddCommand);
            roleCommand.Subcommands.Add(roleRemoveCommand);

            rootCommand.Subcommands.Add(createCommand);
            rootCommand.Subcommands.Add(readCommand);
            rootCommand.Subcommands.Add(listCommand);
            rootCommand.Subcommands.Add(tierCommand);
            rootCommand.Subcommands.Add(roleCommand);
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

        private static async Task HandleListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.Account> accounts = await client.Account.ListAsync();

            Output.WriteTable(
                accounts,
                nameof(Contracts.Account.Name),
                nameof(Contracts.Account.Tier),
                nameof(Contracts.Account.Roles),
                nameof(Contracts.Account.CreatedDate),
                nameof(Contracts.Account.ModifiedDate));
        }

        private static async Task HandleTierAsync(string name, AccountTier tier, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Account account = await client.Account.UpdateTierAsync(name, tier);
            Console.WriteLine(account.ToJsonString(indented: true));
        }

        private static async Task HandleRoleAddAsync(string name, string role, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Account account = await client.Account.AddRoleAsync(name, role);
            Console.WriteLine(account.ToJsonString(indented: true));
        }

        private static async Task HandleRoleRemoveAsync(string name, string role, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Account account = await client.Account.RemoveRoleAsync(name, role);
            Console.WriteLine(account.ToJsonString(indented: true));
        }
    }
}
