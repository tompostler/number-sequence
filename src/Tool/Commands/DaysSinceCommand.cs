using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class DaysSinceCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("days-since", "Create and manage days since alerts.");

            Argument<string> idArgument = new("daysSinceId") { Description = "The id of the days since." };
            Option<bool> rawOption = new("--raw") { Description = "Show raw json object(s) instead of the nicer summary format." };

            Command createCommand = new("create", "Create a new days since.")
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
            rootCommand.Subcommands.Add(createCommand);

            Command editCommand = new("edit", "Edit an existing days since.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                rawOption,
            };
            editCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string id = parseResult.GetRequiredValue(idArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleEditAsync(id, raw, stamp, verbosity);
                });
            rootCommand.Subcommands.Add(editCommand);

            Command getCommand = new("get", "Get an existing days since.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                rawOption,
            };
            getCommand.Aliases.Add("show");
            getCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string id = parseResult.GetRequiredValue(idArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleGetAsync(id, raw, stamp, verbosity);
                });
            rootCommand.Subcommands.Add(getCommand);

            Command listCommand = new("list", "Get existing days sinces.")
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
            rootCommand.Subcommands.Add(listCommand);


            Command eventCommand = new("event", "Create and manage days since events that reset the counter.");
            Argument<long> eventIdArgument = new("eventId") { Description = "The id of the event." };

            Command eventCreateCommand = new("create", "Create a new days since event.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                rawOption,
            };
            eventCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string id = parseResult.GetRequiredValue(idArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleEventCreateAsync(id, raw, stamp, verbosity);
                });

            Command eventEditCommand = new("edit", "Edit an existing days since event.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                eventIdArgument,
                rawOption,
            };
            eventEditCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string id = parseResult.GetRequiredValue(idArgument);
                    long eventId = parseResult.GetRequiredValue(eventIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleEventEditAsync(id, eventId, raw, stamp, verbosity);
                });

            eventCommand.Subcommands.Add(eventCreateCommand);
            eventCommand.Subcommands.Add(eventEditCommand);
            rootCommand.Subcommands.Add(eventCommand);


            return rootCommand;
        }

        private static async Task HandleCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.DaysSince daysSince = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Id = Input.GetString(nameof(daysSince.Id), Guid.NewGuid().ToString().Split('-').Last()),
                FriendlyName = Input.GetString(nameof(daysSince.FriendlyName)),
            };

            string answer = Input.GetString("Do you want to set Value, or the ValueLines? Default is Value, or enter any text to do ValueLines");
            if (answer == null)
            {
                daysSince.Value = Input.GetString(nameof(daysSince.Value));
            }
            else
            {
                daysSince.ValueLine1 = Input.GetString(nameof(daysSince.ValueLine1));
                daysSince.ValueLine2 = Input.GetString(nameof(daysSince.ValueLine2));
                daysSince.ValueLine3 = Input.GetString(nameof(daysSince.ValueLine3));
                daysSince.ValueLine4 = Input.GetString(nameof(daysSince.ValueLine4));
            }

            daysSince = await client.DaysSince.CreateAsync(daysSince);
            Console.WriteLine(daysSince.ToJsonString(indented: true));
        }

        private static async Task HandleEditAsync(string id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.DaysSince daysSince = await client.DaysSince.GetAsync(id);

            daysSince.FriendlyName = Input.GetString(nameof(daysSince.FriendlyName), daysSince.FriendlyName);

            string answer = Input.GetString("Do you want to update Value, or the ValueLines? Default is Value, or enter any text to do ValueLines");
            if (answer == null)
            {
                daysSince.Value = Input.GetString(nameof(daysSince.Value), daysSince.Value);
                daysSince.ValueLine1 = null;
                daysSince.ValueLine2 = null;
                daysSince.ValueLine3 = null;
                daysSince.ValueLine4 = null;
            }
            else
            {
                daysSince.Value = null;
                daysSince.ValueLine1 = Input.GetString(nameof(daysSince.ValueLine1), daysSince.ValueLine1);
                daysSince.ValueLine2 = Input.GetString(nameof(daysSince.ValueLine2), daysSince.ValueLine2);
                daysSince.ValueLine3 = Input.GetString(nameof(daysSince.ValueLine3), daysSince.ValueLine3);
                daysSince.ValueLine4 = Input.GetString(nameof(daysSince.ValueLine4), daysSince.ValueLine4);
            }

            daysSince = await client.DaysSince.UpdateAsync(daysSince);
            PrintDaysSince(daysSince, raw);
        }

        private static async Task HandleGetAsync(string id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.DaysSince daysSince = await client.DaysSince.GetAsync(id);
            PrintDaysSince(daysSince, raw);
        }

        private static async Task HandleListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.DaysSince> daysSinces = await client.DaysSince.ListAsync();

            PrintDaysSinces(daysSinces.ToArray());
        }

        private static async Task HandleEventCreateAsync(string id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.DaysSince daysSince = await client.DaysSince.GetAsync(id);
            PrintDaysSince(daysSince, raw);

            Contracts.DaysSinceEvent daysSinceEvent = new()
            {
                EventDate = Input.GetDateOnly(nameof(daysSinceEvent.EventDate), DateOnly.FromDateTime(DateTime.UtcNow)),
                Description = Input.GetString(nameof(daysSinceEvent.Description)),
            };
            daysSince.Events.Add(daysSinceEvent);

            daysSince = await client.DaysSince.UpdateAsync(daysSince);
            PrintDaysSince(daysSince, raw);
        }

        private static async Task HandleEventEditAsync(string id, long eventId, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.DaysSince daysSince = await client.DaysSince.GetAsync(id);
            Contracts.DaysSinceEvent daysSinceEvent = daysSince.Events.Single(x => x.Id == eventId);

            daysSinceEvent.EventDate = Input.GetDateOnly(nameof(daysSinceEvent.EventDate), DateOnly.FromDateTime(DateTime.UtcNow));
            daysSinceEvent.Description = Input.GetString(nameof(daysSinceEvent.Description));

            daysSince = await client.DaysSince.UpdateAsync(daysSince);
            PrintDaysSince(daysSince, raw);
        }

        private static void PrintDaysSince(Contracts.DaysSince daysSince, bool raw)
        {
            if (raw)
            {
                Console.WriteLine(daysSince.ToJsonString(indented: true));
            }
            else
            {
                // When displaying a days since by default, nicely output the data instead of dumping the json out.
                // Primarily for days since with more events (as it reduces the amount of scrolling necessary).
                Console.WriteLine();

                Console.WriteLine("Summary:");
                PrintDaysSinces(daysSince);

                Console.WriteLine($"Events ({daysSince.Events.Count}):");
                Output.WriteTable(
                    daysSince.Events
                        .OrderByDescending(x => x.EventDate)
                        .ThenBy(x => x.Id)
                        .Select(x => new
                        {
                            x.Id,
                            x.EventDate,
                            AgoDays = (int)DateTime.UtcNow.Subtract(x.EventDate.ToDateTime(new(), DateTimeKind.Utc)).TotalDays,
                            x.Description,
                        }),
                    nameof(Contracts.DaysSinceEvent.Id),
                    nameof(Contracts.DaysSinceEvent.EventDate),
                    "AgoDays",
                    nameof(Contracts.DaysSinceEvent.Description));
            }
        }

        private static void PrintDaysSinces(params Contracts.DaysSince[] daysSinces)
        {
            Output.WriteTable(
                daysSinces.Select(x => new
                {
                    x.Id,
                    x.FriendlyName,
                    x.Value,
                    x.LastOccurrence,
                    AgoDays = (int)DateTime.UtcNow.Subtract(x.LastOccurrence.ToDateTime(new(), DateTimeKind.Utc)).TotalDays,
                    EventCount = x.Events.Count,
                }),
                nameof(Contracts.DaysSince.Id),
                nameof(Contracts.DaysSince.FriendlyName),
                nameof(Contracts.DaysSince.Value),
                nameof(Contracts.DaysSince.LastOccurrence),
                "AgoDays",
                "EventCount");
        }
    }
}
