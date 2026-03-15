using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Ledger
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
        public DateOnly DueDate { get; set; }

        /// <summary>
        /// See <see cref="Business"/>.
        /// </summary>
        public Business Business { get; set; }

        /// <summary>
        /// See <see cref="Customer"/>.
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// The acutal line items on the invoice.
        /// </summary>
        public IList<InvoiceLine> Lines { get; set; }

        /// <summary>
        /// The payments applied to this invoice.
        /// </summary>
        public IList<InvoicePayment> Payments { get; set; }

        /// <summary>
        /// The date the invoice has been fully paid. Set automatically when payments cover the total.
        /// </summary>
        public DateOnly? PaidDate { get; set; }

        /// <summary>
        /// The total amount due on the invoice.
        /// </summary>
        [NotMapped]
        public decimal Total => this.Lines?.Sum(x => x.Total) ?? 0;

        /// <summary>
        /// The total amount paid across all payments.
        /// </summary>
        [NotMapped]
        public decimal TotalPaid => this.Payments?.Sum(x => x.Amount) ?? 0;

        /// <summary>
        /// The remaining balance after payments.
        /// </summary>
        [NotMapped]
        public decimal Balance => this.Total - this.TotalPaid;

        /// <summary>
        /// A human-friendly identifier combining the invoice id and process attempt.
        /// </summary>
        [NotMapped]
        public string FriendlyId => $"{this.Id:0000}-{this.ProccessAttempt:00}";

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
        /// If the <see cref="ProcessedAt"/> is cleared, this will be used to indicate the number of times it has been processed.
        /// </summary>
        public long ProccessAttempt { get; set; }
        /// <summary>
        /// When the invoice was processed for PDF generation.
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; set; }
        /// <summary>
        /// Will automatically reprocess one the <see cref="ModifiedDate"/> is more than 14d ago.
        /// Also will override the value of one-off processing determined by <see cref="ReadyForProcessing"/>.
        /// </summary>
        public bool ReprocessRegularly { get; set; }
    }
}
