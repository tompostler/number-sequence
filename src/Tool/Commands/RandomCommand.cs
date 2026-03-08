using System.CommandLine;
using System.Security.Cryptography;
using System.Text;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class RandomCommand
    {
        private static NsTcpWtfClient CreateClient(ParseResult parseResult, Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
            => new(new Logger<NsTcpWtfClient>(parseResult.GetRequiredValue(verbosityOption)), EmptyTokenProvider.GetAsync, parseResult.GetRequiredValue(stampOption));

        private static int? SeedToInt(string seed)
        {
            if (string.IsNullOrEmpty(seed))
            {
                return null;
            }

            byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(seed));
            return BitConverter.ToInt32(hash);
        }

        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command command = new("random", "Get random data.");

            // 8ball
            Command eightBallCommand = new("8ball", "Get a magic 8-ball response.") { stampOption, verbosityOption };
            eightBallCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.Get8BallAsync(cancellationToken)));
            command.Subcommands.Add(eightBallCommand);

            // coin
            Command coinCommand = new("coin", "Flip a coin.") { stampOption, verbosityOption };
            coinCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetCoinFlipAsync(cancellationToken)));
            command.Subcommands.Add(coinCommand);

            // gibbs
            Option<string> gibbsNameOption = new("--name") { Description = "The name to use for the rule." };
            Command gibbsCommand = new("gibbs", "Get a Gibbs rule.") { stampOption, verbosityOption, gibbsNameOption };
            gibbsCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetGibbsRuleAsync(parseResult.GetValue(gibbsNameOption), cancellationToken)));
            command.Subcommands.Add(gibbsCommand);

            // guid
            Command guidCommand = new("guid", "Get a new guid.") { stampOption, verbosityOption };
            guidCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetGuidAsync(cancellationToken)));
            command.Subcommands.Add(guidCommand);

            // name, name-moby, name-ubuntu
            Option<string> seedOption = new("--seed") { Description = "Seed to use for the randomness. Hashed with MD5 to an int. (string)" };

            Command nameCommand = new("name", "Get a random name.") { stampOption, verbosityOption, seedOption };
            nameCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetNameAsync(SeedToInt(parseResult.GetValue(seedOption)), cancellationToken)));
            command.Subcommands.Add(nameCommand);

            Command nameMobyCommand = new("name-moby", "Get a random Moby name.") { stampOption, verbosityOption, seedOption };
            nameMobyCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetNameMobyAsync(SeedToInt(parseResult.GetValue(seedOption)), cancellationToken)));
            command.Subcommands.Add(nameMobyCommand);

            Command nameUbuntuCommand = new("name-ubuntu", "Get a random Ubuntu release name.") { stampOption, verbosityOption, seedOption };
            nameUbuntuCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetNameUbuntuAsync(SeedToInt(parseResult.GetValue(seedOption)), cancellationToken)));
            command.Subcommands.Add(nameUbuntuCommand);

            // no
            Option<int?> noValueOption = new("--value") { Description = "The value to use." };
            Command noCommand = new("no", "Get a no.") { stampOption, verbosityOption, noValueOption };
            noCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetNoAsync(parseResult.GetValue(noValueOption), cancellationToken)));
            command.Subcommands.Add(noCommand);

            // razor
            Option<string> razorNameOption = new("--name") { Description = "The name of the razor to get." };
            Command razorCommand = new("razor", "Get a philosophical razor.") { stampOption, verbosityOption, razorNameOption };
            razorCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetPhilosophicalRazorAsync(parseResult.GetValue(razorNameOption), cancellationToken)));
            command.Subcommands.Add(razorCommand);

            // wot
            Option<int?> wotValueOption = new("--value") { Description = "The value to use." };
            Command wotCommand = new("wot", "Get a Wheel of Time intro.") { stampOption, verbosityOption, wotValueOption };
            wotCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetWheelOfTimeIntroAsync(parseResult.GetValue(wotValueOption), cancellationToken)));
            command.Subcommands.Add(wotCommand);

            // xkcd
            Command xkcdCommand = new("xkcd", "Get an XKCD comic reference.") { stampOption, verbosityOption };
            xkcdCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetXkcdAsync(cancellationToken)));
            command.Subcommands.Add(xkcdCommand);

            // identicon
            Option<string> identiconNameOption = new("--name") { Description = "The name to use for the identicon." };
            Option<int?> identiconSizeOption = new("--size") { Description = "The size of the identicon in pixels." };
            Option<FileInfo> identiconOutputOption = new Option<FileInfo>("--output") { Description = "Path to write the output file. Format inferred from extension (.png or .svg)." }.AcceptLegalFilePathsOnly();
            Command identiconCommand = new("identicon", "Generate an identicon image.") { stampOption, verbosityOption, identiconNameOption, identiconSizeOption, identiconOutputOption };
            identiconCommand.SetAction(async (parseResult, cancellationToken) =>
            {
                NsTcpWtfClient client = CreateClient(parseResult, stampOption, verbosityOption);
                string name = parseResult.GetValue(identiconNameOption);
                int? size = parseResult.GetValue(identiconSizeOption);
                FileInfo output = parseResult.GetRequiredValue(identiconOutputOption);
                string ext = output.Extension.ToLowerInvariant();
                if (ext == ".png")
                {
                    byte[] png = await client.Random.GetIdenticonPngAsync(name, size, cancellationToken);
                    await File.WriteAllBytesAsync(output.FullName, png, cancellationToken);
                    Console.WriteLine($"Written {png.Length:N0} bytes (image/png) to {output.FullName}");
                }
                else if (ext == ".svg")
                {
                    string svg = await client.Random.GetIdenticonSvgAsync(name, size, cancellationToken);
                    await File.WriteAllTextAsync(output.FullName, svg, cancellationToken);
                    Console.WriteLine($"Written {svg.Length:N0} chars (image/svg+xml) to {output.FullName}");
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported output format '{ext}'. Use .png or .svg.");
                }
            });
            command.Subcommands.Add(identiconCommand);

            // numbers

            Command bitCommand = new("bit", "Get a random 1-bit number (0-1).") { stampOption, verbosityOption };
            bitCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetULong01Async(cancellationToken)));
            command.Subcommands.Add(bitCommand);

            Command crumbCommand = new("crumb", "Get a random 2-bit number (0-3).") { stampOption, verbosityOption };
            crumbCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetULong02Async(cancellationToken)));
            command.Subcommands.Add(crumbCommand);

            Command nibbleCommand = new("nibble", "Get a random 4-bit number (0-15).") { stampOption, verbosityOption };
            nibbleCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetULong04Async(cancellationToken)));
            command.Subcommands.Add(nibbleCommand);

            Command byteCommand = new("byte", "Get a random 8-bit number (0-255).") { stampOption, verbosityOption };
            byteCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetULong08Async(cancellationToken)));
            command.Subcommands.Add(byteCommand);

            Command shortCommand = new("short", "Get a random 16-bit number (0-65535).") { stampOption, verbosityOption };
            shortCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetULong16Async(cancellationToken)));
            command.Subcommands.Add(shortCommand);

            Command intCommand = new("int", "Get a random 32-bit number (0-4294967295).") { stampOption, verbosityOption };
            intCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetULong32Async(cancellationToken)));
            command.Subcommands.Add(intCommand);

            Command longCommand = new("long", "Get a random 64-bit number (0-18446744073709551615).") { stampOption, verbosityOption };
            longCommand.SetAction(async (parseResult, cancellationToken) =>
                Console.WriteLine(await CreateClient(parseResult, stampOption, verbosityOption).Random.GetULong64Async(cancellationToken)));
            command.Subcommands.Add(longCommand);

            return command;
        }
    }
}
