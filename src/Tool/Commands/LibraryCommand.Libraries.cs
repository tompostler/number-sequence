using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LibraryCommand
    {
        private static async Task HandleLibraryCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Library library = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Name = PromptRequiredString(nameof(library.Name)),
                Description = PromptString(nameof(library.Description)),
            };

            library = await client.Library.CreateLibraryAsync(library);
            Console.WriteLine(library.ToJsonString(indented: true));
        }

        private static async Task HandleLibraryGetAsync(long libraryId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Library library = await client.Library.GetLibraryAsync(libraryId);
            Console.WriteLine(library.ToJsonString(indented: true));
        }

        private static async Task HandleLibraryEditAsync(long libraryId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Library library = await client.Library.GetLibraryAsync(libraryId);
            library.Name = PromptRequiredString(nameof(library.Name), library.Name);
            library.Description = PromptString(nameof(library.Description), library.Description);

            library = await client.Library.UpdateLibraryAsync(library);
            Console.WriteLine(library.ToJsonString(indented: true));
        }

        private static async Task HandleLibraryDeleteAsync(long libraryId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Library library = await client.Library.GetLibraryAsync(libraryId);
            if (!Input.GetBool($"Delete library [{library.Name}] with every item, copy, loan and location in it? This can't be undone"))
            {
                Console.WriteLine("Not deleted.");
                return;
            }

            await client.Library.DeleteLibraryAsync(libraryId);
            Console.WriteLine($"Deleted library [{library.Name}].");
        }

        private static async Task HandleLibraryListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Library> libraries = await client.Library.GetLibrariesAsync();
            WriteLibraries(libraries);
        }

        private static async Task HandleShareSetAsync(long? libraryId, string accountName, LibraryPermission permission, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            LibraryShare share = await client.Library.PutShareAsync(id, new LibraryShare { AccountName = accountName, Permission = permission });
            Console.WriteLine(share.ToJsonString(indented: true));
        }

        private static async Task HandleShareDeleteAsync(long? libraryId, string accountName, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            await client.Library.DeleteShareAsync(id, accountName);
            Console.WriteLine($"Removed [{accountName}]'s share of library [{id}].");
        }

        private static async Task HandleShareListAsync(long? libraryId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            List<LibraryShare> shares = await client.Library.GetSharesAsync(id);
            Console.WriteLine();
            if (shares.Count == 0)
            {
                Console.WriteLine("Not shared with anyone.");
                return;
            }
            Output.WriteTable(
                shares,
                nameof(LibraryShare.AccountName),
                nameof(LibraryShare.Permission),
                nameof(LibraryShare.CreatedDate));
        }
    }
}
