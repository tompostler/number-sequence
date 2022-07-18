using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
    }
}
