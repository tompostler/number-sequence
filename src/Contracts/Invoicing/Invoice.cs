using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TcpWtf.NumberSequence.Contracts.Invoicing
{
    /// <summary>
    /// An invoice.
    /// </summary>
    public sealed class Invoice
    {
        /// <summary>
        /// See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; }

        /// <summary>
        /// The id of the invoice. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The title of the invoice to be displayed as the header. If not specified, will be the invoice id.
        /// </summary>
        [MaxLength(128)]
        public string Title { get; set; }

        /// <summary>
        /// An optional description at the top of the invoice beneath the title.
        /// </summary>
        [MaxLength(512)]
        public string Description { get; set; }

        /// <summary>
        /// The date the invoice is due.
        /// </summary>
        [Required]
        public DateTimeOffset DueDate { get; set; }

        /// <summary>
        /// See <see cref="InvoiceBusiness"/>.
        /// </summary>
        [Required]
        public InvoiceBusiness Business { get; set; }

        /// <summary>
        /// See <see cref="InvoiceCustomer"/>.
        /// </summary>
        [Required]
        public InvoiceCustomer Customer { get; set; }

        /// <summary>
        /// The acutal line items on the invoice.
        /// </summary>
        public IList<InvoiceLine> Lines { get; set; }

        /// <summary>
        /// The date the invoice has been paid.
        /// </summary>
        public DateTimeOffset? PaidDate { get; set; }

        /// <summary>
        /// Additional payment information, if desired.
        /// </summary>
        [MaxLength(64)]
        public string PaidDetails { get; set; }

        /// <summary>
        /// The total amount due on the invoice.
        /// </summary>
        [NotMapped]
        public decimal Total => this.Lines?.Sum(x => x.Total) ?? 0;

        /// <summary>
        /// The date the invoice was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the invoice was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }

        /// <summary>
        /// Indicates if the invoice is ready for PDF generation.
        /// </summary>
        public bool ReadyForProcessing { get; set; }
        /// <summary>
        /// When the invoice was processed for PDF generation.
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; set; }
    }
}
