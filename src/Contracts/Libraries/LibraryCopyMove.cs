using System.ComponentModel.DataAnnotations;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// The body of a request to move a <see cref="LibraryCopy"/>.
    /// </summary>
    public sealed class LibraryCopyMove
    {
        /// <summary>
        /// Where the copy now lives, or null for no recorded location. See <see cref="LibraryLocation.Id"/>.
        /// </summary>
        public long? LocationId { get; set; }

        /// <summary>
        /// Optional notes for the history entry.
        /// </summary>
        [MaxLength(256)]
        public string Notes { get; set; }
    }
}
