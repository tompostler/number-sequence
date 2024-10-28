using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// "X days since Y" behavior. Uses UTC DateOnly.
    /// </summary>
    public sealed class DaysSince
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public const byte MaxValueLineWidth = 20;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The id of the days since. Unique in the system. Case insensitive (normalized to lower case). Used in the url.
        /// </summary>
        [Required, MinLength(3), MaxLength(36)]
        public string Id { get; set; }

        /// <summary>
        /// When provided, a way to keep the instance slightly obfuscated in the url but display in the tab title.
        /// </summary>
        [MaxLength(MaxValueLineWidth)]
        public string FriendlyName { get; set; }

        /// <summary>
        /// Will be overridden by the addition of individual events.
        /// If no events, will default to the creation date.
        /// </summary>
        [CustomValidation(typeof(DaysSinceValidation), nameof(DaysSinceValidation.LastOccurrenceValidation))]
        public DateOnly LastOccurrence { get; set; }

        /// <summary>
        /// The individual events that have caused a reset of the counter.
        /// To be reasonable, this is limited based on the account tier.
        /// </summary>
        public IList<DaysSinceEvent> Events { get; set; }

        /// <summary>
        /// The phrase at the end.
        /// </summary>
        [NotMapped, MaxLength(MaxValueLineWidth * 4)]
        public string Value { get; set; }

        /// <summary>
        /// The phrase at the end, parsed and split into max 4 lines by spaces.
        /// If desired, can tune after the initial parsing.
        /// </summary>
        [MaxLength(MaxValueLineWidth)]
        public string ValueLine1 { get; set; }
        /// <summary>
        /// The phrase at the end, parsed and split into max 4 lines by spaces.
        /// If desired, can tune after the initial parsing.
        /// </summary>
        [MaxLength(MaxValueLineWidth)]
        public string ValueLine2 { get; set; }
        /// <summary>
        /// The phrase at the end, parsed and split into max 4 lines by spaces.
        /// If desired, can tune after the initial parsing.
        /// </summary>
        [MaxLength(MaxValueLineWidth)]
        public string ValueLine3 { get; set; }
        /// <summary>
        /// The phrase at the end, parsed and split into max 4 lines by spaces.
        /// If desired, can tune after the initial parsing.
        /// </summary>
        [MaxLength(MaxValueLineWidth)]
        public string ValueLine4 { get; set; }

        /// <summary>
        /// When the days since was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// When the days since was last modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }

        /// <inheritdoc/>
        public override string ToString() => this.ToJsonString();
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class DaysSinceValidation
    {
        public static ValidationResult LastOccurrenceValidation(DateOnly lastOccurrence, ValidationContext _)
        {
            if (lastOccurrence == default)
            {
                return default;
            }

            if (lastOccurrence < DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddYears(-10).UtcDateTime))
            {
                return new ValidationResult("LastOccurrence cannot be older than 10 years from now.");
            }
            else if (lastOccurrence > DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddYears(10).UtcDateTime))
            {
                return new ValidationResult("LastOccurrence cannot be later than 10 years from now.");
            }

            return default;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
