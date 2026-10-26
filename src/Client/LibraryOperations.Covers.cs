using TcpWtf.NumberSequence.Contracts.Libraries;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LibraryOperations
    {
        /// <summary>
        /// Set an item's cover, replacing any it has, from an image or from a lookup result's cover urls. The server
        /// re-encodes it. Returns the item without its copies.
        /// </summary>
        public async Task<LibraryItem> SetItemCoverAsync(long libraryId, long itemId, LibraryCover cover, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"library/libraries/{libraryId}/items/{itemId}/cover")
                {
                    Content = cover.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryItem>(cancellationToken);
        }

        /// <summary>
        /// Remove an item's cover.
        /// </summary>
        public async Task DeleteItemCoverAsync(long libraryId, long itemId, CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"library/libraries/{libraryId}/items/{itemId}/cover"),
                cancellationToken);
        }
    }
}
