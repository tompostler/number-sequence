using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// A snapshot of a count's value at the time of an increment.
    /// </summary>
    public sealed class CountEvent
    {
        /// <summary>
        /// Auto-incrementing id.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="Count.Account"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string Account { get; set; }

        /// <summary>
        /// See <see cref="Count.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string CountName { get; set; }

        /// <summary>
        /// The absolute value of the count after the increment.
        /// </summary>
        public ulong Value { get; set; }

        /// <summary>
        /// The amount that was added in this increment.
        /// </summary>
        public ulong IncrementAmount { get; set; }

        /// <summary>
        /// When the event was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// When the event was last modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }

        /// <summary>
        /// A reference to the count this event is for.
        /// </summary>
        public Count Count { get; set; }
    }
}
