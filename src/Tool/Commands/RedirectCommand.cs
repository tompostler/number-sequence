﻿using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class RedirectCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("redirect", "Create, update, or get redirects.");

            Command createCommand = new("create", "Create a new count.");
            createCommand.SetHandler(HandleCreateAsync, stampOption, verbosityOption);

            Command listCommand = new("list", "Get existing redirects.");
            listCommand.AddAlias("ls");
            listCommand.SetHandler(HandleListAsync, stampOption, verbosityOption);

            rootCommand.AddCommand(createCommand);
            rootCommand.AddCommand(listCommand);
            return rootCommand;
        }

        private static async Task HandleCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Redirect redirect = new()
            {
                Id = Input.GetString(nameof(redirect.Id), defaultVal: Guid.NewGuid().ToString().Split('-').Last()),
                Value = new(Input.GetString(nameof(redirect.Value))),
                Expiration = Input.GetDateTime(nameof(redirect.Expiration), defaultVal: DateTimeOffset.MinValue.AddSeconds(1)),
            };
            if (redirect.Expiration == DateTimeOffset.MinValue.AddSeconds(1))
            {
                redirect.Expiration = default;
            }

            redirect = await client.Redirect.CreateAsync(redirect);
            Console.WriteLine(redirect.ToJsonString(indented: true));
        }

        private static async Task HandleListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.Redirect> redirects = await client.Redirect.ListAsync();

            Output.WriteTable(
                redirects,
                nameof(Contracts.Redirect.Id),
                nameof(Contracts.Redirect.Value),
                nameof(Contracts.Redirect.Hits),
                nameof(Contracts.Redirect.Expiration),
                nameof(Contracts.Redirect.CreatedDate),
                nameof(Contracts.Redirect.ModifiedDate));
        }
    }
}
