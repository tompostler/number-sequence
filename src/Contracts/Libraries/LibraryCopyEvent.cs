using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// An append-only history entry for a copy. Written by the server whenever a copy's location, loan, or status changes.
    /// </summary>
    public sealed class LibraryCopyEvent
    {
        /// <summary>
        /// The id of the event. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="LibraryCopy.Id"/>.
        /// </summary>
        public long CopyId { get; set; }

        /// <summary>
        /// What happened.
        /// </summary>
        [Required, Column(TypeName = "NVARCHAR(16)")]
        public LibraryCopyEventType EventType { get; set; }

        /// <summary>
        /// The location before the change, for moves. Not a foreign key, so history survives the location being deleted.
        /// </summary>
        public long? FromLocationId { get; set; }

        /// <summary>
        /// The location after the change, for moves and acquisitions. Not a foreign key, for the same reason.
        /// </summary>
        public long? ToLocationId { get; set; }

        /// <summary>
        /// The loan involved, for loans and returns.
        /// </summary>
        public long? LoanId { get; set; }

        /// <summary>
        /// The account that made the change, which may be an editor rather than the owner. See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Optional notes.
        /// </summary>
        [MaxLength(256)]
        public string Notes { get; set; }

        /// <summary>
        /// When it happened.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }
    }
}
