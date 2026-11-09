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
        /// The query is a title search of <paramref name="mediaType"/>, or with <paramref name="source"/> that source's id.
        /// </summary>
        private static async Task HandleLookupAsync(string query, LibraryMediaType mediaType, string source, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            List<LibraryLookupResult> results;
            if (!string.IsNullOrWhiteSpace(source))
            {
                results = [await client.Library.GetLookupAsync(source, query)];
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
    }
}
