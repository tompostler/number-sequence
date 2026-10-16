using TcpWtf.NumberSequence.Contracts.Libraries;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LibraryOperations
    {
        /// <summary>
        /// Create a new location, optionally inside another.
        /// </summary>
        public async Task<LibraryLocation> CreateLocationAsync(long libraryId, LibraryLocation location, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/locations")
                {
                    Content = location.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryLocation>(cancellationToken);
        }

        /// <summary>
        /// Get every location in a library, each with its full <see cref="LibraryLocation.Path"/>, sorted by path.
        /// </summary>
        public async Task<List<LibraryLocation>> GetLocationsAsync(long libraryId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{libraryId}/locations"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<LibraryLocation>>(cancellationToken);
        }

        /// <summary>
        /// Rename, renote or move an existing location.
        /// </summary>
        public async Task<LibraryLocation> UpdateLocationAsync(long libraryId, LibraryLocation location, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"library/libraries/{libraryId}/locations")
                {
                    Content = location.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryLocation>(cancellationToken);
        }

        /// <summary>
        /// Delete an empty location. Refused (409) while copies or other locations are still in it.
        /// </summary>
        public async Task DeleteLocationAsync(long libraryId, long locationId, CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"library/libraries/{libraryId}/locations/{locationId}"),
                cancellationToken);
        }
    }
}
