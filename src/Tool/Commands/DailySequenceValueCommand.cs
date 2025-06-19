using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class DailySequenceValueCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("daily-sequence-value", "Create, update, or get daily sequence values (DSVs).");
            rootCommand.AddAlias("dsv");

            Argument<string> categoryArgument = new("category", "The category of the DSV.");
            Argument<DateOnly> dateArgument = new("date", "The date of the DSV.");
            Argument<decimal> valueArgument = new("value", "The value of the DSV.");

            Command createCommand = new("create", "Create a new DSV for today.");
            createCommand.AddArgument(categoryArgument);
            createCommand.AddArgument(valueArgument);
            createCommand.SetHandler(HandleCreateAsync, categoryArgument, valueArgument, stampOption, verbosityOption);

            Command createAskCommand = new("create-ask", "Create a new DSV, asking for the properties.");
            createAskCommand.SetHandler(HandleCreateWithInputsAsync, stampOption, verbosityOption);

            Command getCommand = new("get", "Get an existing DSV to see its properties.");
            getCommand.AddAlias("show");
            getCommand.AddArgument(categoryArgument);
            getCommand.AddArgument(dateArgument);
            getCommand.SetHandler(HandleGetAsync, categoryArgument, dateArgument, stampOption, verbosityOption);

            Command listCommand = new("list", "Get existing DSVs.");
            listCommand.AddAlias("ls");
            Argument<string> categoryOptionalArgument = new("category", () => default, "The category of the DSV.");
            listCommand.AddArgument(categoryOptionalArgument);
            listCommand.SetHandler(HandleListAsync, categoryOptionalArgument, stampOption, verbosityOption);

            Command updateCommand = new("update", "Update an existing DSV.");
            updateCommand.AddArgument(categoryArgument);
            updateCommand.AddArgument(dateArgument);
            updateCommand.SetHandler(HandleUpdateAsync, categoryArgument, dateArgument, stampOption, verbosityOption);

            rootCommand.AddCommand(createCommand);
            rootCommand.AddCommand(createAskCommand);
            rootCommand.AddCommand(getCommand);
            rootCommand.AddCommand(listCommand);
            rootCommand.AddCommand(updateCommand);
            return rootCommand;
        }

        private static async Task HandleCreateAsync(string category, decimal value, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.DailySequenceValue dsv = new()
            {
                Category = category,
                EventDate = DateOnly.FromDateTime(DateTime.Now),
                Value = value,
            };

            dsv = await client.DailySequenceValue.CreateAsync(dsv);
            Console.WriteLine(dsv.ToJsonString(indented: true));
        }

        private static async Task HandleCreateWithInputsAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.DailySequenceValue dsv = new()
            {
                Category = Input.GetString(nameof(dsv.Category)),
                EventDate = Input.GetDateOnly(nameof(dsv.EventDate), defaultVal: DateOnly.FromDateTime(DateTime.Now)),
                Value = Input.GetDecimal(nameof(dsv.Value), canDefault: false),
            };

            dsv = await client.DailySequenceValue.CreateAsync(dsv);
            Console.WriteLine(dsv.ToJsonString(indented: true));
        }

        private static async Task HandleGetAsync(string category, DateOnly date, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.DailySequenceValue dsv = await client.DailySequenceValue.GetAsync(category, date);
            Console.WriteLine(dsv.ToJsonString(indented: true));
        }

        private static async Task HandleListAsync(string category, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.DailySequenceValue> dsvs = await client.DailySequenceValue.ListAsync(category);

            Output.WriteTable(
                dsvs,
                nameof(Contracts.DailySequenceValue.Category),
                nameof(Contracts.DailySequenceValue.EventDate),
                nameof(Contracts.DailySequenceValue.Value));
        }

        private static async Task HandleUpdateAsync(string category, DateOnly date, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.DailySequenceValue dsv = await client.DailySequenceValue.GetAsync(category, date);
            Console.WriteLine(dsv.ToJsonString(indented: true));

            dsv.Value = Input.GetDecimal(nameof(dsv.Value), canDefault: false, dsv.Value);

            Console.WriteLine(dsv.ToJsonString(indented: true));
        }
    }
}
