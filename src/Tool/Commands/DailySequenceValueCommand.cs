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
            rootCommand.Aliases.Add("dsv");

            Argument<string> categoryArgument = new("category") { Description = "The category of the DSV." };
            Argument<string> categoryOptionalArgument = new("category")
            {
                Description = "The category of the DSV.",
                DefaultValueFactory = _ => default,
            };
            Argument<DateOnly> dateArgument = new("date") { Description = "The date of the DSV." };
            Argument<decimal> valueArgument = new("value") { Description = "The value of the DSV." };
            Option<int> takeAmountOption = new("--take")
            {
                Description = "How many records of each type to return, within the days of lookback ordered by most recent first.",
                DefaultValueFactory = _ => 20,
            };
            Option<int> daysLookbackOption = new("--days-lookback")
            {
                Description = "How many days of lookback.",
                DefaultValueFactory = _ => 30,
            };

            Command createCommand = new("create", "Create a new DSV for today.")
            {
                stampOption,
                verbosityOption,
                categoryArgument,
                valueArgument,
            };
            createCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string category = parseResult.GetRequiredValue(categoryArgument);
                    decimal value = parseResult.GetRequiredValue(valueArgument);
                    return HandleCreateAsync(category, value, stamp, verbosity);

                });

            Command createAskCommand = new("create-ask", "Create a new DSV, asking for the properties.")
            {
                stampOption,
                verbosityOption,
            };
            createAskCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleCreateWithInputsAsync(stamp, verbosity);
                });

            Command getCommand = new("get", "Get an existing DSV to see its properties.")
            {
                stampOption,
                verbosityOption,
                categoryArgument,
                dateArgument,
            };
            getCommand.Aliases.Add("show");
            getCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string category = parseResult.GetRequiredValue(categoryArgument);
                    DateOnly date = parseResult.GetRequiredValue(dateArgument);
                    return HandleGetAsync(category, date, stamp, verbosity);
                });

            Command listCommand = new("list", "Get existing DSVs.")
            {
                stampOption,
                verbosityOption,
                categoryOptionalArgument,
                takeAmountOption,
                daysLookbackOption,
            };
            listCommand.Aliases.Add("ls");
            listCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string category = parseResult.GetValue(categoryOptionalArgument);
                    int takeAmount = parseResult.GetRequiredValue(takeAmountOption);
                    int daysLookback = parseResult.GetRequiredValue(daysLookbackOption);
                    return HandleListAsync(category, takeAmount, daysLookback, stamp, verbosity);
                });

            Command updateCommand = new("update", "Update an existing DSV.")
            {
                stampOption,
                verbosityOption,
                categoryArgument,
                dateArgument,
            };
            updateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string category = parseResult.GetRequiredValue(categoryArgument);
                    DateOnly date = parseResult.GetRequiredValue(dateArgument);
                    return HandleUpdateAsync(category, date, stamp, verbosity);
                });

            rootCommand.Subcommands.Add(createCommand);
            rootCommand.Subcommands.Add(createAskCommand);
            rootCommand.Subcommands.Add(getCommand);
            rootCommand.Subcommands.Add(listCommand);
            rootCommand.Subcommands.Add(updateCommand);
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

        private static async Task HandleListAsync(string category, int takeAmount, int daysLookback, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.DailySequenceValue> dsvs = await client.DailySequenceValue.ListAsync(category, takeAmount, daysLookback);

            Output.WriteTable(
                dsvs,
                nameof(Contracts.DailySequenceValue.Category),
                nameof(Contracts.DailySequenceValue.EventDate),
                nameof(Contracts.DailySequenceValue.Value),
                nameof(Contracts.DailySequenceValue.OriginalValue));
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
