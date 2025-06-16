using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// An at-most daily sequence to track values for a given category.
    /// </summary>
    public sealed class DailySequenceValue
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
        /// The value.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// When the event occurred.
        /// </summary>
        [Required, CustomValidation(typeof(MiscValidation), nameof(MiscValidation.DateOnlyWithinTenYears))]
        public DateOnly EventDate { get; set; }

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
    }
}
