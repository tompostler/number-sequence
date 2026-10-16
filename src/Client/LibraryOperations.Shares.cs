using TcpWtf.NumberSequence.Contracts.Libraries;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LibraryOperations
    {
        /// <summary>
        /// Get the shares of a library. Owner only.
        /// </summary>
        public async Task<List<LibraryShare>> GetSharesAsync(long libraryId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{libraryId}/shares"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<LibraryShare>>(cancellationToken);
        }

        /// <summary>
        /// Share a library with an account, or change the permission of an existing share. Owner only.
        /// </summary>
        public async Task<LibraryShare> PutShareAsync(long libraryId, LibraryShare share, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"library/libraries/{libraryId}/shares")
                {
                    Content = share.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryShare>(cancellationToken);
        }

        /// <summary>
        /// Remove an account's share of a library. The owner can remove anyone's; anyone can remove their own, which is how
        /// a shared library is left.
        /// </summary>
        public async Task DeleteShareAsync(long libraryId, string accountName, CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"library/libraries/{libraryId}/shares/{Uri.EscapeDataString(accountName)}"),
                cancellationToken);
        }
    }
}
