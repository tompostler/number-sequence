using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// A run of barcode scans at one location, used both to build an initial inventory and to reconcile a location later.
    /// Entries are stored as they are scanned so that a phone locking or reloading mid-run loses nothing.
    /// </summary>
    public sealed class LibraryScanSession
    {
        /// <summary>
        /// The id of the session. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="Library.Id"/>.
        /// </summary>
        public long LibraryId { get; set; }

        /// <summary>
        /// The location being scanned. See <see cref="LibraryLocation.Id"/>.
        /// </summary>
        public long LocationId { get; set; }

        /// <summary>
        /// The account doing the scanning. See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// When the reconcile was applied. Null while the session is still open.
        /// </summary>
        public DateTimeOffset? CompletedDate { get; set; }

        /// <summary>
        /// When the session was started.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }


        /// <summary>
        /// The barcodes scanned in this session.
        /// </summary>
        public ICollection<LibraryScanEntry> Entries { get; set; }
    }
}
