using TcpWtf.NumberSequence.Contracts.Libraries;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LibraryOperations
    {
        /// <summary>
        /// Add a copy of an existing item. It starts active, with an acquired event.
        /// </summary>
        public async Task<LibraryCopy> CreateCopyAsync(long libraryId, LibraryCopy copy, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/copies")
                {
                    Content = copy.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryCopy>(cancellationToken);
        }

        /// <summary>
        /// Get an existing copy.
        /// </summary>
        public async Task<LibraryCopy> GetCopyAsync(long libraryId, long copyId, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"library/libraries/{libraryId}/copies/{copyId}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryCopy>(cancellationToken);
        }

        /// <summary>
        /// Update a copy's descriptive fields, or move it to another item to merge a duplicate. Location, status and loans are
        /// left alone; use <see cref="MoveCopyAsync"/>, <see cref="SetCopyStatusAsync"/>, <see cref="LoanCopyAsync"/> and
        /// <see cref="ReturnCopyAsync"/>.
        /// </summary>
        public async Task<LibraryCopy> UpdateCopyAsync(long libraryId, LibraryCopy copy, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    $"library/libraries/{libraryId}/copies")
                {
                    Content = copy.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryCopy>(cancellationToken);
        }

        /// <summary>
        /// Delete a copy with its loans and history. To keep the history of a copy that's gone, dispose it instead.
        /// </summary>
        public async Task DeleteCopyAsync(long libraryId, long copyId, CancellationToken cancellationToken = default)
        {
            _ = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"library/libraries/{libraryId}/copies/{copyId}"),
                cancellationToken);
        }

        /// <summary>
        /// Move a copy to another location, or to none.
        /// </summary>
        public async Task<LibraryCopy> MoveCopyAsync(long libraryId, long copyId, LibraryCopyMove move, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/copies/{copyId}/move")
                {
                    Content = move.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryCopy>(cancellationToken);
        }

        /// <summary>
        /// Mark a copy missing or disposed, or restore it to active.
        /// </summary>
        public async Task<LibraryCopy> SetCopyStatusAsync(long libraryId, long copyId, LibraryCopyStatusChange change, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/copies/{copyId}/status")
                {
                    Content = change.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryCopy>(cancellationToken);
        }

        /// <summary>
        /// Lend a copy. <see cref="LibraryLoan.LoanedDate"/> defaults to today when left unset.
        /// </summary>
        public async Task<LibraryLoan> LoanCopyAsync(long libraryId, long copyId, LibraryLoan loan, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/copies/{copyId}/loan")
                {
                    Content = loan.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryLoan>(cancellationToken);
        }

        /// <summary>
        /// Close a copy's open loan. <see cref="LibraryLoanReturn.ReturnedDate"/> defaults to today when left unset.
        /// </summary>
        public async Task<LibraryLoan> ReturnCopyAsync(long libraryId, long copyId, LibraryLoanReturn loanReturn, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    $"library/libraries/{libraryId}/copies/{copyId}/return")
                {
                    Content = loanReturn.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<LibraryLoan>(cancellationToken);
        }
    }
}
