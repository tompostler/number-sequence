using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// A copy lent to someone. A copy has at most one open loan, which is one with no <see cref="ReturnedDate"/>.
    /// </summary>
    public sealed class LibraryLoan
    {
        /// <summary>
        /// The id of the loan. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="LibraryCopy.Id"/>.
        /// </summary>
        public long CopyId { get; set; }

        /// <summary>
        /// Who has it. Always set, even when <see cref="BorrowerAccountName"/> is.
        /// </summary>
        [Required, MaxLength(64)]
        public string BorrowerName { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Optionally, the borrower's account. See <see cref="Account.Name"/>.
        /// </summary>
        [MaxLength(64)]
        public string BorrowerAccountName { get; set; }

        /// <summary>
        /// When the copy was lent.
        /// </summary>
        [Required]
        public DateOnly LoanedDate { get; set; }

        /// <summary>
        /// When the copy is expected back, if agreed.
        /// </summary>
        public DateOnly? DueDate { get; set; }

        /// <summary>
        /// When the copy came back. Null while the loan is open.
        /// </summary>
        public DateOnly? ReturnedDate { get; set; }

        /// <summary>
        /// Optional notes.
        /// </summary>
        [MaxLength(256)]
        public string Notes { get; set; }

        /// <summary>
        /// The date the loan was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the loan was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }


        /// <summary>
        /// The copy lent out.
        /// </summary>
        public LibraryCopy Copy { get; set; }
    }
}
