using System;
using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class DaysSinceCommand
    {
        public static Command Create(Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("days-since", "Create and manage days since alerts.");

            Argument<string> idArgument = new("daysSinceId", "The id of the days since.");
            Option<bool> rawOption = new("--raw", "Show raw json object(s) instead of the nicer summary format.");

            Command createCommand = new("create", "Create a new days since.");
            createCommand.SetHandler(HandleCreateAsync, verbosityOption);
            rootCommand.AddCommand(createCommand);

            Command editCommand = new("edit", "Edit an existing days since.");
            editCommand.AddArgument(idArgument);
            editCommand.AddOption(rawOption);
            editCommand.SetHandler(HandleEditAsync, idArgument, rawOption, verbosityOption);
            rootCommand.AddCommand(editCommand);

            Command getCommand = new("get", "Get an existing days since.");
            getCommand.AddAlias("show");
            getCommand.AddArgument(idArgument);
            getCommand.AddOption(rawOption);
            getCommand.SetHandler(HandleGetAsync, idArgument, rawOption, verbosityOption);
            rootCommand.AddCommand(getCommand);

            Command listCommand = new("list", "Get existing days sinces.");
            listCommand.AddAlias("ls");
            listCommand.SetHandler(HandleListAsync, verbosityOption);
            rootCommand.AddCommand(listCommand);


            Command eventCommand = new("event", "Create and manage days since events that reset the counter.");

            Command eventCreateCommand = new("create", "Create a new days since event.");
            eventCreateCommand.AddArgument(idArgument);
            eventCreateCommand.AddOption(rawOption);
            eventCreateCommand.SetHandler(HandleEventCreateAsync, idArgument, rawOption, verbosityOption);

            Command eventEditCommand = new("edit", "Edit an existing days since event.");
            Argument<long> eventIdArgument = new("eventId", "The id of the event.");
            eventEditCommand.AddArgument(idArgument);
            eventEditCommand.AddArgument(eventIdArgument);
            eventEditCommand.AddOption(rawOption);
            eventEditCommand.SetHandler(HandleEventEditAsync, idArgument, eventIdArgument, rawOption, verbosityOption);

            eventCommand.AddCommand(eventCreateCommand);
            eventCommand.AddCommand(eventEditCommand);
            rootCommand.AddCommand(eventCommand);


            return rootCommand;
        }

        private static async Task HandleCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

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

        private static async Task HandleEditAsync(string id, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

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

        private static async Task HandleGetAsync(string id, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.DaysSince daysSince = await client.DaysSince.GetAsync(id);
            PrintDaysSince(daysSince, raw);
        }

        private static async Task HandleListAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            List<Contracts.DaysSince> daysSinces = await client.DaysSince.ListAsync();

            PrintDaysSinces(daysSinces.ToArray());
        }

        private static async Task HandleEventCreateAsync(string id, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
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

        private static async Task HandleEventEditAsync(string id, long eventId, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

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

                Console.WriteLine("Events:");
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
