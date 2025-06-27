using System.CommandLine;
using System.Security.Cryptography;
using System.Text;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class RandomCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command command = new("random", "Get random data.");

            Argument<string> randomTypeArg = new Argument<string>("type", "The type of random to get. Pick from the supported values.")
                .FromAmong(
                    "8ball",
                    "coin",
                    "gibbs",
                    "guid",
                    "name",
                    "name_moby",
                    "name_ubuntu",
                    "razor",
                    "wot",
                    "xkcd",

                    "bit",
                    "crumb",
                    "nibble",
                    "byte",
                    "short",
                    "int",
                    "long"
                );
            command.AddArgument(randomTypeArg);

            Option<string> nameOpt = new("--name", "If provided and the API supports it, the name to use for the randomness.");
            command.AddOption(nameOpt);

            Option<string> seedOpt = new("--seed", "If provided and the API supports it, the seed to use for the randomness. Hashed with MD5 to an int. (string)");
            command.AddOption(seedOpt);

            Option<int?> valueOpt = new("--value", "If provided and the API supports it, the value to use for the randomness. (int)");
            command.AddOption(valueOpt);

            command.SetHandler(HandleAsync, randomTypeArg, nameOpt, seedOpt, valueOpt, stampOption, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(string type, string nameStr, string seedStr, int? value, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, stamp);

            // If there's a seed, convert it to an int by hashing it
            int? seed = default;
            if (!string.IsNullOrEmpty(seedStr))
            {
                byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(seedStr));
                seed = BitConverter.ToInt32(hash);
            }

            object response = type switch
            {
                "8ball" => await client.Random.Get8BallAsync(),
                "coin" => await client.Random.GetCoinFlipAsync(),
                "gibbs" => await client.Random.GetGibbsRuleAsync(nameStr),
                "guid" => await client.Random.GetGuidAsync(),
                "name" => await client.Random.GetNameAsync(seed),
                "name_moby" => await client.Random.GetNameMobyAsync(seed),
                "name_ubuntu" => await client.Random.GetNameUbuntuAsync(seed),
                "razor" => await client.Random.GetPhilosophicalRazorAsync(nameStr),
                "wot" => await client.Random.GetWheelOfTimeIntroAsync(value),
                "xkcd" => await client.Random.GetXkcdAsync(),

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
