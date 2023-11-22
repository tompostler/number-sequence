using System.ComponentModel.DataAnnotations;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// Short url behavior: ns.tcp.wtf/r/{id}
    /// </summary>
    public sealed class Redirect
    {
        /// <summary>
        /// See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The id of the redirect. Unique in the system. Case insensitive (normalized to lower case).
        /// </summary>
        [Required, MinLength(3), MaxLength(36)]
        public string Id { get; set; }

        /// <summary>
        /// The target of the redirect.
        /// </summary>
        [Required, Length(12, 4096)]
        [CustomValidation(typeof(RedirectValidation), nameof(RedirectValidation.ValueValidation))]
        public string Value { get; set; }

        /// <summary>
        /// If set, a point at which the redirect should stop being served.
        /// </summary>
        [CustomValidation(typeof(RedirectValidation), nameof(RedirectValidation.ExpirationValidation))]
        public DateTimeOffset? Expiration { get; set; }

        /// <summary>
        /// Approximate number of times the redirect has been used.
        /// </summary>
        public ulong Hits { get; set; }

        /// <summary>
        /// When the redirect was created.
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// When the redirect was last modified.
        /// </summary>
        public DateTimeOffset ModifiedDate { get; set; }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class RedirectValidation
    {
        public static ValidationResult ValueValidation(string value, ValidationContext _)
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri _))
            {
                return new ValidationResult("Value needs to be a valid Absolute Uri.");
            }

            return default;
        }

        public static ValidationResult ExpirationValidation(DateTimeOffset? expirationDate, ValidationContext _)
        {
            if (expirationDate < DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return new ValidationResult("ExpirationDate cannot be in the past.");
            }
            else if (expirationDate > DateTimeOffset.UtcNow.AddYears(10))
            {
                return new ValidationResult("ExpirationDate cannot be later than 10 years from now.");
            }

            return default;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
