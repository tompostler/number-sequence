using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using number_sequence.Models;

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
                public const string Pdf = "pdf";
            }

            /// <summary>
            /// Pdf template names
            /// </summary>
            public static class PT
            {
                public const string ChiroCanine = "chiro-canine";
                public const string ChiroEquine = "chiro-equine";
                public const string InvoicePostler = "invoice-postler";
            }
        }

        private readonly BlobServiceClient blobServiceClient;

        public NsStorage(
            IOptions<Options.Storage> options)
        {
            this.blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
        }

        public BlobClient GetBlobClient(EmailDocument emailDocument)
        {
            BlobContainerClient blobContainerClient = this.blobServiceClient.GetBlobContainerClient(C.CN.Pdf);
            return blobContainerClient.GetBlobClient($"{(emailDocument.CreatedDate == default ? DateTimeOffset.UtcNow : emailDocument.CreatedDate).Year}/{EnsureEndsWithPdf(emailDocument.AttachmentName ?? emailDocument.Id)}");
        }

        private static string EnsureEndsWithPdf(string input) => input.EndsWith(".pdf") ? input : input + ".pdf";
    }
}
