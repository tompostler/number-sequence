using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// The individual events that reset the days since counter.
    /// </summary>
    public sealed class DaysSinceEvent
    {
        /// <summary>
        /// The id of the event. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// When the event occurred.
        /// </summary>
        [Required, CustomValidation(typeof(DaysSinceValidation), nameof(DaysSinceValidation.LastOccurrenceValidation))]
        public DateOnly EventDate { get; set; }

        /// <summary>
        /// What happened that caused the days since to be reset.
        /// </summary>
        [Required, MinLength(4), MaxLength(256)]
        public string Description { get; set; }

        /// <summary>
        /// The date the line was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the line was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }


        /// <summary>
        /// A reference to the days since this event is for.
        /// </summary>
        public DaysSince DaysSince { get; set; }

        /// <inheritdoc/>
        public override string ToString() => this.ToJsonString();
    }
}
