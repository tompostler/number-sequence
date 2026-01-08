using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class CountCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("count", "Create, update, or get counts. The namesake of the program.");

            Command createCommand = new("create", "Create a new count.")
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

            Argument<string> countNameArgument = new("name") { Description = "The name of the count." };
            Option<bool> bareOption = new("--bare") { Description = "When used, will only return the count instead of the full count json object." };
            Option<bool> basesOption = new("--bases") { Description = "When used, will return the count object with all the bases." };

            Command getCommand = new("get", "Get an existing count to see its properties.")
            {
                stampOption,
                verbosityOption,
                countNameArgument,
                bareOption,
                basesOption,
            };
            getCommand.Aliases.Add("show");
            getCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string name = parseResult.GetRequiredValue(countNameArgument);
                    bool bare = parseResult.GetValue(bareOption);
                    bool bases = parseResult.GetValue(basesOption);
                    return HandleGetAsync(name, bare, bases, stamp, verbosity);
                });

            Command incrementCommand = new("increment", "Increment a count by one.")
            {
                stampOption,
                verbosityOption,
                countNameArgument,
                bareOption,
                basesOption,
            };
            incrementCommand.Aliases.Add("inc");
            incrementCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string name = parseResult.GetRequiredValue(countNameArgument);
                    bool bare = parseResult.GetValue(bareOption);
                    bool bases = parseResult.GetValue(basesOption);
                    return HandleIncrementAsync(name, bare, bases, stamp, verbosity);
                });

            Argument<ulong> incrementByAmountArgument = new("amount") { Description = "The amount to increment the count by." };
            Command incrementByCommand = new("increment-by", "Increment a count by a specific amount.")
            {
                stampOption,
                verbosityOption,
                countNameArgument,
                incrementByAmountArgument,
                bareOption,
                basesOption,
            };
            incrementByCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string name = parseResult.GetRequiredValue(countNameArgument);
                    ulong incrementByAmount = parseResult.GetRequiredValue(incrementByAmountArgument);
                    bool bare = parseResult.GetValue(bareOption);
                    bool bases = parseResult.GetValue(basesOption);
                    return HandleIncrementByAsync(name, incrementByAmount, bare, bases, stamp, verbosity);
                });

            Command listCommand = new("list", "Get existing counts.")
            {
                stampOption,
                verbosityOption,
            };
            listCommand.Aliases.Add("ls");
            listCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleListAsync(stamp, verbosity);
                });

            rootCommand.Subcommands.Add(createCommand);
            rootCommand.Subcommands.Add(getCommand);
            rootCommand.Subcommands.Add(incrementCommand);
            rootCommand.Subcommands.Add(incrementByCommand);
            rootCommand.Subcommands.Add(listCommand);
            return rootCommand;
        }

        private static async Task HandleCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Count count = new()
            {
                Name = Input.GetString(nameof(count.Name)),
                Value = Input.GetULong(nameof(count.Value)),
            };

            count = await client.Count.CreateAsync(count);
            Console.WriteLine(count.ToJsonString(indented: true));
        }

        private static async Task HandleGetAsync(string name, bool bare, bool bases, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Count count = await client.Count.GetAsync(name);

            if (bare)
            {
                Console.WriteLine(count.Value);
            }
            else if (bases)
            {
                Console.WriteLine(CountWithBases.From(count).ToJsonString(indented: true));
            }
            else
            {
                Console.WriteLine(count.ToJsonString(indented: true));
            }
        }

        private static async Task HandleIncrementAsync(string name, bool bare, bool bases, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Count count = await client.Count.IncrementAsync(name);

            if (bare)
            {
                Console.WriteLine(count.Value);
            }
            else if (bases)
            {
                Console.WriteLine(CountWithBases.From(count).ToJsonString(indented: true));
            }
            else
            {
                Console.WriteLine(count.ToJsonString(indented: true));
            }
        }

        private static async Task HandleIncrementByAsync(string name, ulong amount, bool bare, bool bases, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Count count = await client.Count.IncrementByAmountAsync(name, amount);

            if (bare)
            {
                Console.WriteLine(count.Value);
            }
            else if (bases)
            {
                Console.WriteLine(CountWithBases.From(count).ToJsonString(indented: true));
            }
            else
            {
                Console.WriteLine(count.ToJsonString(indented: true));
            }
        }

        private static async Task HandleListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.Count> counts = await client.Count.ListAsync();

            Output.WriteTable(
                counts,
                nameof(Contracts.Count.Name),
                nameof(Contracts.Count.Value),
                nameof(Contracts.Count.CreatedDate),
                nameof(Contracts.Count.ModifiedDate));
        }
    }
}
