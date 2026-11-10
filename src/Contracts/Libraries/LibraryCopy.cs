using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// One physical object belonging to a <see cref="LibraryItem"/>. Edition details live here rather than on a separate
    /// edition table, so two copies of the same paperback simply repeat them.
    /// </summary>
    public sealed class LibraryCopy
    {
        /// <summary>
        /// The id of the copy. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="Library.Id"/>. Must match the item's library; stored here so barcode lookups and scoping are a
        /// single query.
        /// </summary>
        public long LibraryId { get; set; }

        /// <summary>
        /// See <see cref="LibraryItem.Id"/>.
        /// </summary>
        public long ItemId { get; set; }

        /// <summary>
        /// The physical format, such as Hardcover, Paperback, DVD, Blu-ray, or 4K.
        /// </summary>
        [MaxLength(32)]
        public string Format { get; set; }

        /// <summary>
        /// The ISBN, UPC, or EAN printed on the copy, normalized to digits only (ISBN-10 is stored as ISBN-13).
        /// </summary>
        [MaxLength(32)]
        public string Barcode { get; set; }

        /// <summary>
        /// Where the copy lives, if recorded. See <see cref="LibraryLocation.Id"/>.
        /// </summary>
        public long? LocationId { get; set; }

        /// <summary>
        /// When the copy was acquired.
        /// </summary>
        public DateOnly? AcquiredDate { get; set; }

        /// <summary>
        /// Whether the copy is still part of the library.
        /// </summary>
        [Required, Column(TypeName = "NVARCHAR(16)")]
        public LibraryCopyStatus Status { get; set; }

        /// <summary>
        /// Optional notes on this one copy, such as a signature, an inscription or missing pieces.
        /// </summary>
        [MaxLength(1024)]
        public string Notes { get; set; }

        /// <summary>
        /// The date the copy was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the copy was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }


        /// <summary>
        /// The item this is a copy of.
        /// </summary>
        public LibraryItem Item { get; set; }

        /// <summary>
        /// Where the copy lives.
        /// </summary>
        public LibraryLocation Location { get; set; }

        /// <summary>
        /// Every loan of this copy, including the open one if any.
        /// </summary>
        public ICollection<LibraryLoan> Loans { get; set; }

        /// <summary>
        /// The history of this copy.
        /// </summary>
        public ICollection<LibraryCopyEvent> Events { get; set; }
    }
}
