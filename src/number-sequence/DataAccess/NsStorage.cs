using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace number_sequence.DataAccess
{
    public sealed class NsStorage
    {
        private readonly BlobServiceClient blobServiceClient;
        private readonly ILogger<NsStorage> logger;

        public NsStorage(
            IOptions<Options.Storage> options,
            ILogger<NsStorage> logger)
        {
            this.blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
            this.logger = logger;
        }

        public async IAsyncEnumerable<BlobClient> EnumerateAllBlobsForLatexJobAsync(string jobId, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            uint countBlobs = 0;
            BlobContainerClient blobContainerClient = this.blobServiceClient.GetBlobContainerClient("latex");
            await foreach (BlobItem blob in blobContainerClient.GetBlobsAsync(prefix: jobId + '/', cancellationToken: cancellationToken))
            {
                countBlobs++;
                yield return blobContainerClient.GetBlobClient(blob.Name);
            }
            this.logger.LogInformation($"Enumerated {countBlobs} blobs in {blobContainerClient.Uri}/latex/{jobId}/");
        }

        public BlobClient GetBlobClientForLatexJob(string jobId, string blobPath)
        {
            BlobContainerClient blobContainerClient = this.blobServiceClient.GetBlobContainerClient("latex");
            return blobContainerClient.GetBlobClient(Path.Combine(jobId, blobPath));
        }
    }
}
