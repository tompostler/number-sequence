using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// A token is used for actual authentication.
    /// </summary>
    public sealed class Token
    {
        /// <summary>
        /// The name of the account the token is generated for.
        /// </summary>
        [Required, MinLength(3), MaxLength(64)]
        public string Account { get; set; }

        /// <summary>
        /// The tier of the account. Copied from the account at token creation time for convenience.
        /// </summary>
        [Required, Column(TypeName = "NVARCHAR(8)")]
        public AccountTier AccountTier { get; set; }

        /// <summary>
        /// The account's password. Used when creating a token to verify ownership.
        /// </summary>
        /// <remarks>
        /// When stored in the database, it'll be a random value.
        /// </remarks>
        [Required, MinLength(32), MaxLength(128)]
        public string Key { get; set; }

        /// <summary>
        /// The name you wish to be associated with the token. This allows you to have more than one token.
        /// </summary>
        [Required, MinLength(3), MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// The actual token value to be used on authentication calls.
        /// </summary>
        [MaxLength(512)]
        public string Value { get; set; }

        /// <summary>
        /// When the token was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// When the token expires. Default is 90 days.
        /// </summary>
        [CustomValidation(typeof(TokenValidation), nameof(TokenValidation.ExpirationValidation))]
        public DateTimeOffset ExpirationDate { get; set; } = DateTimeOffset.UtcNow.AddDays(90);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class TokenValidation
    {
        public static ValidationResult ExpirationValidation(DateTimeOffset expirationDate, ValidationContext _)
        {
            if (expirationDate < DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return new ValidationResult("ExpirationDate cannot be in the past.");
            }
            else if (expirationDate.Year > DateTimeOffset.UtcNow.AddYears(10).Year)
            {
                return new ValidationResult("ExpirationDate cannot be later than 10 years from now.");
            }

            return default;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
