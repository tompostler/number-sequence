using TcpWtf.NumberSequence.Contracts.Libraries;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LibraryOperations
    {
        /// <summary>
        /// Create a new item. Add its copies with <see cref="CreateCopyAsync"/>.
        /// </summary>
        public async Task<LibraryItem> CreateItemAsync(long libraryId, LibraryItem item, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/items")
                {
                    Content = item.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryItem>(cancellationToken);
        }

        /// <summary>
        /// Get an item with every copy, each copy's loans and its history.
        /// </summary>
        public async Task<LibraryItem> GetItemAsync(long libraryId, long itemId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{libraryId}/items/{itemId}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryItem>(cancellationToken);
        }

        /// <summary>
        /// Search a library. Each item comes without <see cref="LibraryItem.Summary"/>, <see cref="LibraryItem.ExternalIds"/>
        /// and <see cref="LibraryItem.Attributes"/>, and each copy's <see cref="LibraryCopy.Loans"/> holds only its open loan;
        /// <see cref="GetItemAsync"/> has the rest.
        /// <para>
        /// <c>q</c> is matched against title, subtitle, creators, series and notes, and against copy barcodes if it parses as one.
        /// <c>locationId</c> includes everything inside that location. <c>loanedOut</c> keeps only items with a copy on loan.
        /// <c>sort</c> is shelf order when not given.
        /// </para>
        /// </summary>
        public async Task<List<LibraryItem>> GetItemsAsync(
            long libraryId,
            string q = null,
            LibraryMediaType? mediaType = null,
            long? locationId = null,
            bool loanedOut = false,
            int skip = 0,
            int take = 100,
            LibraryItemSort? sort = null,
            CancellationToken cancellationToken = default)
        {
            List<string> queryParams = [$"skip={skip}", $"take={take}"];
            if (sort.HasValue)
            {
                queryParams.Add($"sort={sort.Value}");
            }
            if (!string.IsNullOrWhiteSpace(q))
            {
                queryParams.Add($"q={Uri.EscapeDataString(q)}");
            }
            if (mediaType.HasValue)
            {
                queryParams.Add($"mediaType={mediaType.Value}");
            }
            if (locationId.HasValue)
            {
                queryParams.Add($"locationId={locationId.Value}");
            }
            if (loanedOut)
            {
                queryParams.Add("loanedOut=true");
            }

            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{libraryId}/items?{string.Join("&", queryParams)}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<LibraryItem>>(cancellationToken);
        }

        /// <summary>
        /// Items whose <see cref="LibraryItem.ExternalIds"/> has this entry, in the same shape as <see cref="GetItemsAsync"/>.
        /// This is how a lookup result finds the item it's already in the library as.
        /// </summary>
        public async Task<List<LibraryItem>> GetItemsByExternalIdAsync(long libraryId, string key, string value, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{libraryId}/items?externalIdKey={Uri.EscapeDataString(key)}&externalIdValue={Uri.EscapeDataString(value)}&take=20"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<LibraryItem>>(cancellationToken);
        }

        /// <summary>
        /// Every series name in use in a library, sorted.
        /// </summary>
        public async Task<List<string>> GetSeriesAsync(long libraryId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{libraryId}/series"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<string>>(cancellationToken);
        }

        /// <summary>
        /// Set one series on several items, each with its own number. A blank name takes them out of any series. Returns
        /// the items as saved.
        /// </summary>
        public async Task<List<LibraryItem>> SetSeriesAsync(long libraryId, LibrarySeriesAssign assign, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"library/libraries/{libraryId}/series")
                {
                    Content = assign.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<LibraryItem>>(cancellationToken);
        }

        /// <summary>
        /// Update an existing item's descriptive fields.
        /// </summary>
        public async Task<LibraryItem> UpdateItemAsync(long libraryId, LibraryItem item, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"library/libraries/{libraryId}/items")
                {
                    Content = item.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryItem>(cancellationToken);
        }

        /// <summary>
        /// Delete an item with its copies, loans and history. To keep history of a copy that's gone, dispose it instead.
        /// </summary>
        public async Task DeleteItemAsync(long libraryId, long itemId, CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"library/libraries/{libraryId}/items/{itemId}"),
                cancellationToken);
        }
    }
}
