using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class DailySequenceValueConfigCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("daily-sequence-value-config", "Create, update, or get daily sequence value configs (DSVCs).");
            rootCommand.AddAlias("dsvc");

            Argument<string> categoryArgument = new("category", "The category of the DSVC.");

            Command createCommand = new("create", "Create a new DSVC.");
            createCommand.SetHandler(HandleCreateAsync, stampOption, verbosityOption);

            Command getCommand = new("get", "Get an existing DSVC to see its properties.");
            getCommand.AddAlias("show");
            getCommand.AddArgument(categoryArgument);
            getCommand.SetHandler(HandleGetAsync, categoryArgument, stampOption, verbosityOption);

            Command listCommand = new("list", "Get existing DSVCs.");
            listCommand.AddAlias("ls");
            listCommand.SetHandler(HandleListAsync, stampOption, verbosityOption);

            Command updateCommand = new("update", "Update an existing DSVC.");
            updateCommand.AddArgument(categoryArgument);
            updateCommand.SetHandler(HandleUpdateAsync, categoryArgument, stampOption, verbosityOption);

            rootCommand.AddCommand(createCommand);
            rootCommand.AddCommand(getCommand);
            rootCommand.AddCommand(listCommand);
            rootCommand.AddCommand(updateCommand);
            return rootCommand;
        }

        private static async Task HandleCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.DailySequenceValueConfig dsvc = new()
            {
                Category = Input.GetString(nameof(dsvc.Category)),
                NegativeDeltaMax = Input.GetDecimalNullable(nameof(dsvc.NegativeDeltaMax)),
                NegativeDeltaMin = Input.GetDecimalNullable(nameof(dsvc.NegativeDeltaMin)),
                PositiveDeltaMax = Input.GetDecimalNullable(nameof(dsvc.PositiveDeltaMax)),
                PositiveDeltaMin = Input.GetDecimalNullable(nameof(dsvc.PositiveDeltaMin)),
            };

            dsvc = await client.DailySequenceValueConfig.CreateAsync(dsvc);
            Console.WriteLine(dsvc.ToJsonString(indented: true));
        }

        private static async Task HandleGetAsync(string category, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.DailySequenceValueConfig dsvc = await client.DailySequenceValueConfig.GetAsync(category);
            Console.WriteLine(dsvc.ToJsonString(indented: true));
        }

        private static async Task HandleListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.DailySequenceValueConfig> dsvcs = await client.DailySequenceValueConfig.ListAsync();

            Output.WriteTable(
                dsvcs,
                nameof(Contracts.DailySequenceValueConfig.Category),
                nameof(Contracts.DailySequenceValueConfig.NegativeDeltaMax),
                nameof(Contracts.DailySequenceValueConfig.NegativeDeltaMin),
                nameof(Contracts.DailySequenceValueConfig.PositiveDeltaMax),
                nameof(Contracts.DailySequenceValueConfig.PositiveDeltaMin));
        }

        private static async Task HandleUpdateAsync(string category, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.DailySequenceValueConfig dsvc = await client.DailySequenceValueConfig.GetAsync(category);
            Console.WriteLine(dsvc.ToJsonString(indented: true));

            dsvc.NegativeDeltaMax = Input.GetDecimalNullable(nameof(dsvc.NegativeDeltaMax), canDefault: true, defaultVal: dsvc.NegativeDeltaMax);
            dsvc.NegativeDeltaMin = Input.GetDecimalNullable(nameof(dsvc.NegativeDeltaMin), canDefault: true, defaultVal: dsvc.NegativeDeltaMin);
            dsvc.PositiveDeltaMax = Input.GetDecimalNullable(nameof(dsvc.PositiveDeltaMax), canDefault: true, defaultVal: dsvc.PositiveDeltaMax);
            dsvc.PositiveDeltaMin = Input.GetDecimalNullable(nameof(dsvc.PositiveDeltaMin), canDefault: true, defaultVal: dsvc.PositiveDeltaMin);

            dsvc = await client.DailySequenceValueConfig.UpdateAsync(dsvc);
            Console.WriteLine(dsvc.ToJsonString(indented: true));
        }
    }
}
