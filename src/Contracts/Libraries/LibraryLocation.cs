using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// A place copies live, such as a room or a shelf. Locations nest via <see cref="ParentLocationId"/>, so a shelf can sit
    /// inside a bookcase inside a room.
    /// </summary>
    public sealed class LibraryLocation
    {
        /// <summary>
        /// The id of the location. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="Library.Id"/>.
        /// </summary>
        public long LibraryId { get; set; }

        /// <summary>
        /// The containing location, if any. Must be in the same library.
        /// </summary>
        public long? ParentLocationId { get; set; }

        /// <summary>
        /// The location name, such as "Office" or "Shelf 3". Only needs to be unique among its siblings to be useful.
        /// </summary>
        [Required, MaxLength(64)]
        public string Name { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Optional notes about the location.
        /// </summary>
        [MaxLength(256)]
        public string Notes { get; set; }

        /// <summary>
        /// The date the location was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the location was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
