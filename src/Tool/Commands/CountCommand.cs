using System;
using System.CommandLine;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class CountCommand
    {
        public static Command Create(Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("count", "Create, update, or get counts. The namesake of the program.");

            Command createCommand = new("create", "Create a new count.");
            createCommand.SetHandler(HandleCreateAsync, verbosityOption);
            
            Argument<string> countNameArgument = new("name", "The name of the count.");
            Option<bool> bareOption = new("--bare", "When used, will only return the count instead of the full count json object.");

            Command getCommand = new("get", "Get an existing count to see its properties.");
            getCommand.AddArgument(countNameArgument);
            getCommand.AddOption(bareOption);
            getCommand.SetHandler(HandleGetAsync, countNameArgument, bareOption, verbosityOption);

            Command incrementCommand = new("increment", "Increment a count by one.");
            incrementCommand.AddAlias("inc");
            incrementCommand.AddArgument(countNameArgument);
            incrementCommand.AddOption(bareOption);
            incrementCommand.SetHandler(HandleIncrementAsync, countNameArgument, bareOption, verbosityOption);

            Command incrementByCommand = new("increment-by", "Increment a count by a specific amount.");
            incrementByCommand.AddArgument(countNameArgument);
            Argument<ulong> incrementByAmountArgument = new("amount", "The amount to increment the count by.");
            incrementByCommand.AddArgument(incrementByAmountArgument);
            incrementByCommand.AddOption(bareOption);
            incrementByCommand.SetHandler(HandleIncrementByAsync, countNameArgument, incrementByAmountArgument, bareOption, verbosityOption);

            rootCommand.AddCommand(createCommand);
            rootCommand.AddCommand(getCommand);
            rootCommand.AddCommand(incrementCommand);
            rootCommand.AddCommand(incrementByCommand);
            return rootCommand;
        }

        private static async Task HandleCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Count count = new()
            {
                Name = Input.GetString(nameof(count.Name)),
                Value = Input.GetULong(nameof(count.Value)),
            };

            count = await client.Count.CreateAsync(count);
            Console.WriteLine(count.ToJsonString(indented: true));
        }

        private static async Task HandleGetAsync(string name, bool bare, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Count count = await client.Count.GetAsync(name);

            if (bare)
            {
                Console.WriteLine(count.Value);
            }
            else
            {
                Console.WriteLine(count.ToJsonString(indented: true));
            }
        }

        private static async Task HandleIncrementAsync(string name, bool bare, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Count count = await client.Count.IncrementAsync(name);

            if (bare)
            {
                Console.WriteLine(count.Value);
            }
            else
            {
                Console.WriteLine(count.ToJsonString(indented: true));
            }
        }

        private static async Task HandleIncrementByAsync(string name, ulong amount, bool bare, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Count count = await client.Count.IncrementByAmountAsync(name, amount);

            if (bare)
            {
                Console.WriteLine(count.Value);
            }
            else
            {
                Console.WriteLine(count.ToJsonString(indented: true));
            }
        }
    }
}
