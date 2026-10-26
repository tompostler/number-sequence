using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LibraryCommand
    {
        /// <summary>
        /// For scripting, so it writes the same columns whatever was asked and nothing else: no blank line, no "not found".
        /// An ISBN is looked up as one; anything else is a title search of <paramref name="mediaType"/>, or with
        /// <paramref name="source"/> the query is that source's id.
        /// </summary>
        private static async Task HandleLookupAsync(string query, LibraryMediaType mediaType, string source, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            List<LibraryLookupResult> results;
            if (!string.IsNullOrWhiteSpace(source))
            {
                results = [await client.Library.GetLookupAsync(source, query)];
            }
            else if (LooksLikeIsbn(query))
            {
                results = await client.Library.LookupIsbnAsync(query);
            }
            else
            {
                results = await client.Library.SearchLookupAsync(mediaType, query);
            }

            if (raw)
            {
                Console.WriteLine(results.ToJsonString(indented: true));
                return;
            }
            if (results.Count == 0)
            {
                return;
            }

            Output.WriteTable(
                results.Select(x => new
                {
                    x.Source,
                    x.SourceId,
                    x.MediaType,
                    x.Title,
                    x.Subtitle,
                    x.Creators,
                    x.Year,
                    x.Barcode,
                    x.Format,
                    x.Publisher,
                    x.PublishedYear,
                }),
                "Source",
                "SourceId",
                "MediaType",
                "Title",
                "Subtitle",
                "Creators",
                "Year",
                "Barcode",
                "Format",
                "Publisher",
                "PublishedYear");
        }

        /// <summary>
        /// Ten or thirteen digits (an ISBN-10 may end in X), ignoring dashes and spaces. The server checks it properly; this
        /// only decides between ISBN lookup and title search, so a title like "1984" stays a search.
        /// </summary>
        private static bool LooksLikeIsbn(string query)
        {
            string compact = new(query.Where(x => !char.IsWhiteSpace(x) && x != '-').ToArray());
            return (compact.Length == 13 && compact.All(char.IsAsciiDigit) && (compact.StartsWith("978") || compact.StartsWith("979")))
                || (compact.Length == 10 && compact[..9].All(char.IsAsciiDigit) && (char.IsAsciiDigit(compact[9]) || compact[9] is 'X' or 'x'));
        }
    }
}
