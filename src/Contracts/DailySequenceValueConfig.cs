using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// Configurations for a daily sequence value.
    /// </summary>
    public sealed class DailySequenceValueConfig
    {
        /// <summary>
        /// The name of the account this is for. Populated from the token used for authentication.
        /// </summary>
        /// <remarks>
        /// Set to garbage for the model handling; overridden in the controller.
        /// </remarks>
        [Required, MinLength(3), MaxLength(64)]
        public string Account { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The category of the item. Used to group items together, and makes up the primary key with the date.
        /// </summary>
        [Required, MinLength(3), MaxLength(36)]
        public string Category { get; set; }


        /// <summary>
        /// Limit the negative delta between consecutive values to be at most this value.
        /// </summary>
        public decimal? NegativeDeltaMax { get; set; }

        /// <summary>
        /// Limit the negative delta between consecutive values to be at least this value.
        /// </summary>
        public decimal? NegativeDeltaMin { get; set; }

        /// <summary>
        /// Limit the positive delta between consecutive values to be at most this value.
        /// </summary>
        public decimal? PositiveDeltaMax { get; set; }

        /// <summary>
        /// Limit the positive delta between consecutive values to be at least this value.
        /// </summary>
        public decimal? PositiveDeltaMin { get; set; }


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

        /// <inheritdoc/>
        public override string ToString() => this.ToJsonString();


        /// <summary>
        /// Validates the consistency of the delta range properties and returns an error message if any validation fails.
        /// </summary>
        public string Validate()
        {
            if (this.NegativeDeltaMax.HasValue && this.NegativeDeltaMax > 0)
            {
                return $"{nameof(this.NegativeDeltaMax)} [{this.NegativeDeltaMax}] cannot be positive.";
            }
            if (this.NegativeDeltaMin.HasValue && this.NegativeDeltaMin > 0)
            {
                return $"{nameof(this.NegativeDeltaMin)} [{this.NegativeDeltaMin}] cannot be positive.";
            }
            if (this.NegativeDeltaMin.HasValue && this.NegativeDeltaMax.HasValue && this.NegativeDeltaMin > this.NegativeDeltaMax)
            {
                return $"{nameof(this.NegativeDeltaMin)} [{this.NegativeDeltaMin}] cannot be greater than {nameof(this.NegativeDeltaMax)} [{this.NegativeDeltaMax}].";
            }

            if (this.PositiveDeltaMax.HasValue && this.PositiveDeltaMax > 0)
            {
                return $"{nameof(this.PositiveDeltaMax)} [{this.PositiveDeltaMax}] cannot be negative.";
            }
            if (this.PositiveDeltaMin.HasValue && this.PositiveDeltaMin > 0)
            {
                return $"{nameof(this.PositiveDeltaMin)} [{this.PositiveDeltaMin}] cannot be negative.";
            }
            if (this.PositiveDeltaMin.HasValue && this.PositiveDeltaMax.HasValue && this.PositiveDeltaMin > this.PositiveDeltaMax)
            {
                return $"{nameof(this.PositiveDeltaMin)} [{this.PositiveDeltaMin}] cannot be greater than {nameof(this.PositiveDeltaMax)} [{this.PositiveDeltaMax}].";
            }
            return null;
        }
    }
}
