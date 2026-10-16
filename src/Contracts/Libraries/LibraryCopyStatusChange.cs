using System.ComponentModel.DataAnnotations;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// The body of a request to change a <see cref="LibraryCopy.Status"/>.
    /// </summary>
    public sealed class LibraryCopyStatusChange
    {
        /// <summary>
        /// The new status.
        /// </summary>
        [Required]
        public LibraryCopyStatus Status { get; set; }

        /// <summary>
        /// Optional notes for the history entry, such as who it was given to.
        /// </summary>
        [MaxLength(256)]
        public string Notes { get; set; }
    }
}
