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
            Argument<string> randomTypeArgument = new Argument<string>("type") { Description = "The type of random to get. Pick from the supported values." }
                .AcceptOnlyFromAmong(
                    "8ball",
                    "coin",
                    "gibbs",
                    "guid",
                    "name",
                    "name_moby",
                    "name_ubuntu",
                    "no",
                    "razor",
                    "wot",
                    "xkcd",

                    "bit",
                    "crumb",
                    "nibble",
                    "byte",
                    "short",
                    "int",
                    "long",

                    "identicon_png",
                    "identicon_svg"
                );

            Option<string> nameOption = new("--name") { Description = "If provided and the API supports it, the name to use for the randomness." };

            Option<string> seedOption = new("--seed") { Description = "If provided and the API supports it, the seed to use for the randomness. Hashed with MD5 to an int. (string)" };

            Option<int?> valueOption = new("--value") { Description = "If provided and the API supports it, the value to use for the randomness. (int)" };

            Option<FileInfo> outputOption = new Option<FileInfo>("--output") { Description = "If provided and the API supports it, the file to write the output to." }.AcceptLegalFilePathsOnly();

            Command command = new("random", "Get random data.")
            {
                stampOption,
                verbosityOption,
                randomTypeArgument,
                nameOption,
                seedOption,
                valueOption,
                outputOption,
            };
            command.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string type = parseResult.GetValue(randomTypeArgument);
                    string name = parseResult.GetValue(nameOption);
                    string seed = parseResult.GetValue(seedOption);
                    int? value = parseResult.GetValue(valueOption);
                    FileInfo output = parseResult.GetValue(outputOption);
                    return HandleAsync(type, name, seed, value, output, stamp, verbosity);
                });

            return command;
        }

        private static async Task HandleAsync(string type, string nameStr, string seedStr, int? value, FileInfo output, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), EmptyTokenProvider.GetAsync, stamp);

            // If there's a seed, convert it to an int by hashing it
            int? seed = default;
            if (!string.IsNullOrEmpty(seedStr))
            {
                byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(seedStr));
                seed = BitConverter.ToInt32(hash);
            }

            if (type == "identicon_png")
            {
                byte[] png = await client.Random.GetIdenticonPngAsync(nameStr, value);
                await File.WriteAllBytesAsync(output.FullName, png);
                Console.WriteLine($"Written {png.Length:N0} bytes (image/png) to {output.FullName}");
                return;
            }

            if (type == "identicon_svg")
            {
                string svg = await client.Random.GetIdenticonSvgAsync(nameStr, value);
                await File.WriteAllTextAsync(output.FullName, svg);
                Console.WriteLine($"Written {svg.Length:N0} chars (image/svg+xml) to {output.FullName}");
                return;
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
                "no" => await client.Random.GetNoAsync(value),
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
