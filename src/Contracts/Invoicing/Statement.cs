using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Invoicing
{
    /// <summary>
    /// An statement.
    /// </summary>
    [CustomValidation(typeof(StatementValidation), nameof(StatementValidation.InvoiceRangeValidation))]
    public sealed class Statement
    {
        /// <summary>
        /// See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; }

        /// <summary>
        /// The id of the statement. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Start date of the statement period.
        /// </summary>
        [Required, CustomValidation(typeof(MiscValidation), nameof(MiscValidation.DateOnlyWithinTenYears))]
        public DateOnly InvoiceStartDate { get; set; }

        /// <summary>
        /// End date of the statement period.
        /// </summary>
        [Required, CustomValidation(typeof(MiscValidation), nameof(MiscValidation.DateOnlyWithinTenYears))]
        public DateOnly InvoiceEndDate { get; set; }

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
        /// The acutal line items on the statement.
        /// </summary>
        public IList<Invoice> Invoices { get; set; }

        /// <summary>
        /// The total amount of the invoices.
        /// </summary>
        [NotMapped]
        public decimal TotalBilled => this.Invoices?.Sum(x => x.Total) ?? 0;

        /// <summary>
        /// The total amount of the paid invoices.
        /// </summary>
        [NotMapped]
        public decimal TotalPaid => this.Invoices?.Where(x => x.PaidDate.HasValue).Sum(x => x.Total) ?? 0;

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
        /// Whether to search for invoices by due date instead of created date.
        /// </summary>
        public bool SearchByDueDate { get; set; }

        /// <summary>
        /// Indicates if the statement is ready for PDF generation.
        /// </summary>
        public bool ReadyForProcessing { get; set; }
        /// <summary>
        /// If the <see cref="ProcessedAt"/> is cleared, this will be used to indicate the number of times it has been processed.
        /// </summary>
        public long ProccessAttempt { get; set; }
        /// <summary>
        /// When the statement was processed for PDF generation.
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static class StatementValidation
        {
            public static ValidationResult InvoiceRangeValidation(Statement statement, ValidationContext _)
            {
                if (statement.InvoiceEndDate <= statement.InvoiceStartDate)
                {
                    return new ValidationResult("InvoiceStartDate must be before InvoiceEndDate.");
                }

                return default;
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
