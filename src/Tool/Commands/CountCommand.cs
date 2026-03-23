using ScottPlot;
using System.CommandLine;
using TcpWtf.NumberSequence.Client;
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
            createCommand.AddCreateAliases();
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

            Command readCommand = new("read", "Read an existing count to see its properties.")
            {
                stampOption,
                verbosityOption,
                countNameArgument,
                bareOption,
                basesOption,
            };
            readCommand.AddReadAliases();
            readCommand.SetAction(
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
            incrementByCommand.Aliases.Add("inc-by");
            incrementByCommand.Aliases.Add("ib");
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

            Command listCommand = new("list", "List existing counts.")
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

            Option<DateTimeOffset?> fromOption = new("--from") { Description = "Filter events from this date (inclusive)." };
            Option<DateTimeOffset?> toOption = new("--to") { Description = "Filter events to this date (inclusive)." };
            Option<FileInfo> chartOption = new Option<FileInfo>("--chart") { Description = "Generate a PNG chart of the count value over time and save to the specified file path." }.AcceptLegalFileNamesOnly();
            Command eventsCommand = new("events", "List events for a count, optionally filtered by date range.")
            {
                stampOption,
                verbosityOption,
                countNameArgument,
                fromOption,
                toOption,
                chartOption,
            };
            eventsCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string name = parseResult.GetRequiredValue(countNameArgument);
                    DateTimeOffset? from = parseResult.GetValue(fromOption);
                    DateTimeOffset? to = parseResult.GetValue(toOption);
                    FileInfo chart = parseResult.GetValue(chartOption);
                    return HandleEventsAsync(name, from, to, chart, stamp, verbosity);
                });

            Argument<bool> overflowDropsOldestArgument = new("drops-oldest") { Description = "When true, oldest events are dropped. When false, increments are rejected." };
            Command setOverflowCommand = new("set-overflow", "Set the overflow behavior for count events.")
            {
                stampOption,
                verbosityOption,
                countNameArgument,
                overflowDropsOldestArgument,
            };
            setOverflowCommand.AddUpdateAliases();
            setOverflowCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string name = parseResult.GetRequiredValue(countNameArgument);
                    bool dropsOldest = parseResult.GetRequiredValue(overflowDropsOldestArgument);
                    return HandleSetOverflowAsync(name, dropsOldest, stamp, verbosity);
                });

            rootCommand.Subcommands.Add(createCommand);
            rootCommand.Subcommands.Add(readCommand);
            rootCommand.Subcommands.Add(incrementCommand);
            rootCommand.Subcommands.Add(incrementByCommand);
            rootCommand.Subcommands.Add(listCommand);
            rootCommand.Subcommands.Add(eventsCommand);
            rootCommand.Subcommands.Add(setOverflowCommand);
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
                Console.WriteLine(Contracts.CountWithBases.From(count).ToJsonString(indented: true));
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
                Console.WriteLine(Contracts.CountWithBases.From(count).ToJsonString(indented: true));
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
                Console.WriteLine(Contracts.CountWithBases.From(count).ToJsonString(indented: true));
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

        private static async Task HandleEventsAsync(string name, DateTimeOffset? from, DateTimeOffset? to, FileInfo chart, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.CountEvent> events = await client.Count.GetEventsAsync(name, from, to);

            if (chart != null)
            {
                if (events.Count == 0)
                {
                    Console.WriteLine("No events to chart.");
                    return;
                }

                Plot plot = new();
                ScottPlot.Plottables.Scatter scatter = plot.Add.Scatter(events.Select(x => x.CreatedDate.LocalDateTime).ToList(), events.Select(x => x.Value).ToList());
                scatter.LegendText = name;

                _ = plot.Axes.DateTimeTicksBottom();
                _ = plot.HideLegend();
                plot.Axes.TightMargins();
                plot.Axes.SetLimitsY(0, plot.Axes.GetLimits().Top);

                plot.Title($"Count: {name}");
                _ = plot.Add.Annotation($"Generated {DateTime.Now:yyyy-MM-dd HH:mm:ss}", Alignment.UpperLeft);

                chart.Delete();
                _ = plot.SavePng(chart.FullName, 2560, 1440);
                Console.WriteLine($"Wrote {chart.FullName}");
            }
            else
            {
                Output.WriteTable(
                    events,
                    nameof(Contracts.CountEvent.CreatedDate),
                    nameof(Contracts.CountEvent.Value),
                    nameof(Contracts.CountEvent.IncrementAmount));
            }
        }

        private static async Task HandleSetOverflowAsync(string name, bool dropsOldest, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Count count = await client.Count.UpdateOverflowDropsOldestEventsAsync(name, dropsOldest);
            Console.WriteLine(count.ToJsonString(indented: true));
        }
    }
}
