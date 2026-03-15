using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Ledger
{
    /// <summary>
    /// A payment applied to an invoice.
    /// </summary>
    public sealed class InvoicePayment
    {
        /// <summary>
        /// The id of the payment. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; }

        /// <summary>
        /// The invoice this payment is for.
        /// </summary>
        public Invoice Invoice { get; set; }

        /// <summary>
        /// The amount of this payment.
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// The date the payment was received.
        /// </summary>
        [Required]
        public DateOnly PaymentDate { get; set; }

        /// <summary>
        /// Additional payment information, if desired (e.g. payment method, reference number).
        /// </summary>
        [MaxLength(64)]
        public string Details { get; set; }

        /// <summary>
        /// The date the payment was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the payment was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
