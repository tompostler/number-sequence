using System.ComponentModel.DataAnnotations;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// The body of a request to close a copy's open <see cref="LibraryLoan"/>.
    /// </summary>
    public sealed class LibraryLoanReturn
    {
        /// <summary>
        /// When it came back. Defaults to today (UTC) when not set.
        /// </summary>
        public DateOnly? ReturnedDate { get; set; }

        /// <summary>
        /// Optional notes for the history entry.
        /// </summary>
        [MaxLength(256)]
        public string Notes { get; set; }
    }
}
