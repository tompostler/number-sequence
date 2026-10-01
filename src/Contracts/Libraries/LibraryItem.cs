using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// A work in a library, such as a particular book, movie, or game, independent of how many physical copies exist.
    /// A hardcover and a paperback of the same book are one item with two <see cref="LibraryCopy"/> rows.
    /// </summary>
    public sealed class LibraryItem
    {
        /// <summary>
        /// The id of the item. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="Library.Id"/>.
        /// </summary>
        public long LibraryId { get; set; }

        /// <summary>
        /// The kind of item.
        /// </summary>
        [Required, Column(TypeName = "NVARCHAR(32)")]
        public LibraryMediaType MediaType { get; set; }

        /// <summary>
        /// The title.
        /// </summary>
        [Required, MaxLength(256)]
        public string Title { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// An optional subtitle.
        /// </summary>
        [MaxLength(256)]
        public string Subtitle { get; set; }

        /// <summary>
        /// Authors, directors, designers, and so on, semicolon separated.
        /// </summary>
        [MaxLength(512)]
        public string Creators { get; set; }

        /// <summary>
        /// The series this item belongs to, if any.
        /// </summary>
        [MaxLength(128)]
        public string Series { get; set; }

        /// <summary>
        /// The position in <see cref="Series"/>. Decimal to allow for novellas such as 2.5.
        /// </summary>
        public decimal? SeriesNumber { get; set; }

        /// <summary>
        /// The year the work was first released. Edition-specific years go on <see cref="LibraryCopy.PublishedYear"/>.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// A summary or description.
        /// </summary>
        [MaxLength(4000)]
        public string Summary { get; set; }

        /// <summary>
        /// Free-form tags, semicolon separated.
        /// </summary>
        [MaxLength(256)]
        public string Tags { get; set; }

        /// <summary>
        /// Identifiers from external lookup sources, such as an Open Library work key, a TMDB id, or a BoardGameGeek id.
        /// Used to recognize that a newly scanned edition belongs to an existing item.
        /// </summary>
        public Dictionary<string, string> ExternalIds { get; set; }

        /// <summary>
        /// Fields that only apply to some media types, such as page count, runtime, or player count.
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// Whether a cover image is stored for this item.
        /// </summary>
        public bool HasCover { get; set; }

        /// <summary>
        /// The date the item was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the item was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }


        /// <summary>
        /// The physical copies of this item.
        /// </summary>
        public ICollection<LibraryCopy> Copies { get; set; }
    }
}
