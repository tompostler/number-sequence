using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;
using Unlimitedinf.Utilities;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    /// <summary>
    /// Prompts and lookups shared by the library handlers. The <see cref="Input"/> helpers can't leave a value empty or
    /// clear one that's set, and most library fields are optional, so these take a blank answer as "keep what's there" and
    /// a lone <c>-</c> as "clear it".
    /// </summary>
    internal static partial class LibraryCommand
    {
        private const string Clear = "-";

        /// <summary>
        /// The library to act on: <paramref name="libraryId"/> when given, else the caller's only library.
        /// </summary>
        private static async Task<long> ResolveLibraryIdAsync(NsTcpWtfClient client, long? libraryId)
        {
            if (libraryId.HasValue)
            {
                return libraryId.Value;
            }

            List<Library> libraries = await client.Library.GetLibrariesAsync();
            if (libraries.Count == 1)
            {
                return libraries[0].Id;
            }
            if (libraries.Count == 0)
            {
                throw new InvalidOperationException("You have no libraries yet. Create one with 'library create'.");
            }

            WriteLibraries(libraries);
            throw new InvalidOperationException("You can access more than one library, so pass --library <id>.");
        }

        private static async Task<Dictionary<long, string>> GetLocationPathsAsync(NsTcpWtfClient client, long libraryId)
            => (await client.Library.GetLocationsAsync(libraryId)).ToDictionary(x => x.Id, x => x.Path);

        private static string LocationPath(Dictionary<long, string> paths, long? locationId)
            => !locationId.HasValue ? "(none)" : paths.GetValueOrDefault(locationId.Value, $"#{locationId}");

        private static void WriteLibraries(List<Library> libraries)
        {
            Console.WriteLine();
            Output.WriteTable(
                libraries,
                nameof(Library.Id),
                nameof(Library.Name),
                nameof(Library.CallerPermission),
                nameof(Library.AccountName),
                nameof(Library.Description));
        }

        private static void WriteLocations(List<LibraryLocation> locations)
        {
            Console.WriteLine();
            if (locations.Count == 0)
            {
                Console.WriteLine("No locations yet.");
                return;
            }
            Output.WriteTable(
                locations,
                nameof(LibraryLocation.Id),
                nameof(LibraryLocation.Path),
                nameof(LibraryLocation.Notes));
        }

        private static string Hint(string current) => current == null ? string.Empty : $" (default {current}, {Clear} to clear)";

        /// <summary>
        /// An optional string. Blank keeps <paramref name="current"/>; <c>-</c> clears it.
        /// </summary>
        private static string PromptString(string prompt, string current = null)
        {
            Console.Write($"{prompt}{Hint(current)}: ");
            string input = Console.ReadLine()?.Trim();
            return string.IsNullOrEmpty(input) ? current : input == Clear ? null : input;
        }

        /// <summary>
        /// A string that can't be left empty. Blank keeps <paramref name="current"/> when there is one.
        /// </summary>
        private static string PromptRequiredString(string prompt, string current = null)
        {
            while (true)
            {
                Console.Write(current == null ? $"{prompt}: " : $"{prompt} (default {current}): ");
                string input = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(input))
                {
                    return input;
                }
                if (current != null)
                {
                    return current;
                }
            }
        }

        /// <summary>
        /// An optional number or date. Blank keeps <paramref name="current"/>; <c>-</c> clears it; anything unparseable asks
        /// again.
        /// </summary>
        private static T? PromptValue<T>(string prompt, T? current = null)
            where T : struct, IParsable<T>
        {
            while (true)
            {
                Console.Write($"{prompt}{Hint(current?.ToString())}: ");
                string input = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    return current;
                }
                if (input == Clear)
                {
                    return null;
                }
                if (T.TryParse(input, null, out T value))
                {
                    return value;
                }
                Console.WriteLine($"  [{input}] isn't a valid {typeof(T).Name}.");
            }
        }

        /// <summary>
        /// One value of <typeparamref name="TEnum"/>, by name (any case) or number. Blank keeps <paramref name="current"/>.
        /// </summary>
        private static TEnum PromptEnum<TEnum>(string prompt, TEnum current)
            where TEnum : struct, Enum
        {
            string options = string.Join(", ", Enum.GetNames<TEnum>());
            while (true)
            {
                Console.Write($"{prompt} [{options}] (default {current}): ");
                string input = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    return current;
                }
                if (Enum.TryParse(input, ignoreCase: true, out TEnum value) && Enum.IsDefined(value))
                {
                    return value;
                }
                Console.WriteLine($"  [{input}] isn't one of {options}.");
            }
        }
    }
}
