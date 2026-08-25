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

                /// <summary>
                /// Retired container from the latex-based pdf generation pipeline (pre-QuestPDF). Still holds pdfs
                /// for documents processed before the cutover that were never copied into <see cref="Pdf"/>.
                /// </summary>
                public const string Latex = "latex";
            }

            /// <summary>
            /// Pdf template names
            /// </summary>
            public static class PT
            {
                public const string ChiroCanine = "chiro-canine";
                public const string ChiroEquine = "chiro-equine";
                public const string ChiroFeline = "chiro-feline";
            }
        }

        private readonly BlobServiceClient blobServiceClient;

        public NsStorage(
            IOptions<Options.Storage> options)
        {
            this.blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
        }

        public BlobClient GetBlobClient(EmailDocument record)
        {
            BlobContainerClient blobContainerClient = this.blobServiceClient.GetBlobContainerClient(C.CN.Pdf);
            return blobContainerClient.GetBlobClient($"{(record.CreatedDate == default ? DateTimeOffset.UtcNow : record.CreatedDate).Year}/{EnsureEndsWithPdf(record.AttachmentName ?? record.Id)}");
        }

        public BlobClient GetBlobClient(ChiroEmailBatch record)
        {
            BlobContainerClient blobContainerClient = this.blobServiceClient.GetBlobContainerClient(C.CN.Pdf);
            return blobContainerClient.GetBlobClient($"{(record.CreatedDate == default ? DateTimeOffset.UtcNow : record.CreatedDate).Year}/{EnsureEndsWithPdf(record.AttachmentName)}");
        }

        /// <summary>
        /// The pdf output of a latex generation job, at the path convention that pipeline used:
        /// <c>{id}/output/{id}.pdf</c>. Only for <c>LatexPdfMigrationBackgroundService</c>'s one-time copy into
        /// <see cref="C.CN.Pdf"/> - remove alongside it once the migration is complete.
        /// </summary>
        public BlobClient GetLegacyLatexPdfBlobClient(string id)
        {
            BlobContainerClient blobContainerClient = this.blobServiceClient.GetBlobContainerClient(C.CN.Latex);
            return blobContainerClient.GetBlobClient($"{id}/output/{id}.pdf");
        }

        private static string EnsureEndsWithPdf(string input) => input.EndsWith(".pdf") ? input : input + ".pdf";
    }
}
