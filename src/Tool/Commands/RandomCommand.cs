using System;
using System.CommandLine;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class RandomCommand
    {
        public static Command Create(Option<Verbosity> verbosityOption)
        {
            Command command = new("random", "Get random data.");
            Argument<string> randomTypeArg = new Argument<string>("type", "The type of random to get. Pick from the supported values.")
                .FromAmong(
                    "8ball",
                    "guid",
                    "name",
                    "bit",
                    "crumb",
                    "nibble",
                    "byte",
                    "short",
                    "int",
                    "long"
                );
            command.AddArgument(randomTypeArg);
            command.SetHandler(HandleAsync, randomTypeArg, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(string type, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetTokenAsync);

            object response = type switch
            {
                "8ball" => await client.Random.Get8BallAsync(),
                "guid" => await client.Random.GetGuidAsync(),
                "name" => await client.Random.GetNameAsync(),
                "bit" => await client.Random.GetULong01Async(),
                "crumb" => await client.Random.GetULong02Async(),
                "nibble" => await client.Random.GetULong04Async(),
                "byte" => await client.Random.GetULong08Async(),
                "short" => await client.Random.GetULong16Async(),
                "int" => await client.Random.GetULong32Async(),
                "long" => await client.Random.GetULong64Async(),
                _ => throw new NotImplementedException($"{type} was not implemented.")
            };
            Console.WriteLine(response);
        }
    }
}
