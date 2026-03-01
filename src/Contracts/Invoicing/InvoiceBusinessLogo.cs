using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Invoicing
{
    /// <summary>
    /// An optional logo image for a business unit, used in invoice and statement PDFs.
    /// </summary>
    public sealed class InvoiceBusinessLogo
    {
        /// <summary>
        /// See <see cref="InvoiceBusiness.Id"/>. Also the primary key for this table.
        /// </summary>
        public long BusinessId { get; set; }

        /// <summary>
        /// The business this logo belongs to.
        /// </summary>
        public InvoiceBusiness Business { get; set; }

        /// <summary>
        /// The MIME content type of the image (e.g. image/png, image/jpeg).
        /// </summary>
        [Required, MaxLength(64)]
        public string ContentType { get; set; }

        /// <summary>
        /// The raw image bytes. Maximum 64kb.
        /// </summary>
        [Required]
        public byte[] Data { get; set; }

        /// <summary>
        /// The date the logo was first uploaded.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the logo was last updated.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
