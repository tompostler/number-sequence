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
        /// <summary>
        /// Constants
        /// </summary>
        public static class C
        {
            /// <summary>
            /// Container names
            /// </summary>
            public static class CN
            {
                public const string Latex = "latex";
                public const string LatexTemplates = "latex-templates";
            }

            /// <summary>
            /// <see cref="CN.Latex"/> container constants
            /// </summary>
            public static class LBP
            {
                public const string Input = "input";
                public const string Output = "output";
            }

            /// <summary>
            /// <see cref="CN.LatexTemplates"/> container constants
            /// </summary>
            public static class LTBP
            {
                public const string ChiroEquine = "chiro-equine";
            }
        }

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
            BlobContainerClient blobContainerClient = this.blobServiceClient.GetBlobContainerClient(C.CN.Latex);
            await foreach (BlobItem blob in blobContainerClient.GetBlobsAsync(prefix: jobId + '/', cancellationToken: cancellationToken))
            {
                countBlobs++;
                yield return blobContainerClient.GetBlobClient(blob.Name);
            }
            this.logger.LogInformation($"Enumerated {countBlobs} blobs in {blobContainerClient.Uri}/{C.CN.Latex}/{jobId}/");
        }

        public async IAsyncEnumerable<BlobClient> EnumerateAllBlobsForLatexTemplateAsync(string templateName, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            uint countBlobs = 0;
            BlobContainerClient blobContainerClient = this.blobServiceClient.GetBlobContainerClient(C.CN.LatexTemplates);
            await foreach (BlobItem blob in blobContainerClient.GetBlobsAsync(prefix: templateName + '/', cancellationToken: cancellationToken))
            {
                countBlobs++;
                yield return blobContainerClient.GetBlobClient(blob.Name);
            }
            this.logger.LogInformation($"Enumerated {countBlobs} blobs in {blobContainerClient.Uri}/{C.CN.LatexTemplates}/{templateName}/");
        }

        public BlobClient GetBlobClientForLatexJob(string jobId, string blobPath)
        {
            BlobContainerClient blobContainerClient = this.blobServiceClient.GetBlobContainerClient(C.CN.Latex);
            return blobContainerClient.GetBlobClient($"{jobId}/{blobPath}");
        }
    }
}
