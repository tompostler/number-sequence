using TcpWtf.NumberSequence.Contracts.Libraries;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LibraryOperations
    {
        /// <summary>
        /// Create a new library, owned by the caller.
        /// </summary>
        public async Task<Library> CreateLibraryAsync(Library library, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "library/libraries")
                {
                    Content = library.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Library>(cancellationToken);
        }

        /// <summary>
        /// Get an existing library, with the caller's permission on it.
        /// </summary>
        public async Task<Library> GetLibraryAsync(long id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{id}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Library>(cancellationToken);
        }

        /// <summary>
        /// Get every library the caller owns or has been shared, owned first.
        /// </summary>
        public async Task<List<Library>> GetLibrariesAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "library/libraries"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<Library>>(cancellationToken);
        }

        /// <summary>
        /// Rename or redescribe an existing library. Owner only.
        /// </summary>
        public async Task<Library> UpdateLibraryAsync(Library library, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "library/libraries")
                {
                    Content = library.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Library>(cancellationToken);
        }

        /// <summary>
        /// Delete a library with everything in it. Owner only.
        /// </summary>
        public async Task DeleteLibraryAsync(long id, CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"library/libraries/{id}"),
                cancellationToken);
        }
    }
}
