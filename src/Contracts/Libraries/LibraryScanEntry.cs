using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// One barcode read during a <see cref="LibraryScanSession"/>. Scanning the same barcode twice makes two entries, and each
    /// one matches a different copy, so two identical paperbacks need two scans.
    /// </summary>
    public sealed class LibraryScanEntry
    {
        /// <summary>
        /// The id of the entry. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="LibraryScanSession.Id"/>.
        /// </summary>
        public long SessionId { get; set; }

        /// <summary>
        /// The barcode read, normalized the same way as <see cref="LibraryCopy.Barcode"/>.
        /// </summary>
        [Required, MaxLength(32)]
        public string Barcode { get; set; }

        /// <summary>
        /// How the barcode matched when it was scanned.
        /// </summary>
        [Required, Column(TypeName = "NVARCHAR(16)")]
        public LibraryScanResolution Resolution { get; set; }

        /// <summary>
        /// The copy this entry matched, if any. Not a foreign key, so deleting a copy doesn't need to reach into old sessions.
        /// </summary>
        public long? ResolvedCopyId { get; set; }

        /// <summary>
        /// When the barcode was scanned.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }
    }
}
