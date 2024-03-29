﻿using System.CommandLine;
using System.Security.Cryptography;
using System.Text;
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
                    "wot",
                    "bit",
                    "crumb",
                    "nibble",
                    "byte",
                    "short",
                    "int",
                    "long"
                );
            command.AddArgument(randomTypeArg);

            Option<string> seedOpt = new("--seed", "If provided and the API supports it, the seed to use for the randomness. Hashed with MD5 to an int. (string)");
            command.AddOption(seedOpt);

            Option<int?> valueOpt = new("--value", "If provided and the API supports it, the value to use for the randomness. (int)");
            command.AddOption(valueOpt);

            command.SetHandler(HandleAsync, randomTypeArg, seedOpt, valueOpt, verbosityOption);
            return command;
        }

        private static async Task HandleAsync(string type, string seedStr, int? value, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, Program.Stamp);

            // If there's a seed, convert it to an int by hashing it
            int? seed = default;
            if (!string.IsNullOrEmpty(seedStr))
            {
                using var alg = MD5.Create();
                byte[] hash = alg.ComputeHash(Encoding.UTF8.GetBytes(seedStr));
                seed = BitConverter.ToInt32(hash);
            }

            object response = type switch
            {
                "8ball" => await client.Random.Get8BallAsync(),
                "guid" => await client.Random.GetGuidAsync(),
                "name" => await client.Random.GetNameAsync(seed),
                "wot" => await client.Random.GetWheelOfTimeIntroAsync(value),
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
