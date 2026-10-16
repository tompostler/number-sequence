using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LibraryCommand
    {
        private static async Task HandleLocationCreateAsync(long? libraryId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            List<LibraryLocation> locations = await client.Library.GetLocationsAsync(id);
            WriteLocations(locations);

            LibraryLocation location = new()
            {
                Name = PromptRequiredString(nameof(location.Name)),
                ParentLocationId = locations.Count == 0 ? null : PromptValue<long>($"{nameof(location.ParentLocationId)} (blank for none)"),
                Notes = PromptString(nameof(location.Notes)),
            };

            location = await client.Library.CreateLocationAsync(id, location);
            Console.WriteLine(location.ToJsonString(indented: true));
        }

        private static async Task HandleLocationEditAsync(long? libraryId, long locationId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            List<LibraryLocation> locations = await client.Library.GetLocationsAsync(id);
            LibraryLocation location = locations.SingleOrDefault(x => x.Id == locationId)
                ?? throw new InvalidOperationException($"Location [{locationId}] not found in library [{id}].");
            WriteLocations(locations);

            location.Name = PromptRequiredString(nameof(location.Name), location.Name);
            location.ParentLocationId = PromptValue(nameof(location.ParentLocationId), location.ParentLocationId);
            location.Notes = PromptString(nameof(location.Notes), location.Notes);

            location = await client.Library.UpdateLocationAsync(id, location);
            Console.WriteLine(location.ToJsonString(indented: true));
        }

        private static async Task HandleLocationDeleteAsync(long? libraryId, long locationId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            await client.Library.DeleteLocationAsync(id, locationId);
            Console.WriteLine($"Deleted location [{locationId}].");
        }

        private static async Task HandleLocationListAsync(long? libraryId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);
            WriteLocations(await client.Library.GetLocationsAsync(id));
        }
    }
}
