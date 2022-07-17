using System;
using System.ComponentModel.DataAnnotations;
using TcpWtf.NumberSequence.Contracts.Interfaces;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <inheritdoc/>
    public sealed class Token : IToken
    {
        /// <inheritdoc/>
        [Required, MinLength(3), MaxLength(64)]
        public string Account { get; set; }

        /// <inheritdoc/>
        public AccountTier AccountTier { get; set; }

        /// <inheritdoc/>
        [Required, MinLength(32), MaxLength(128)]
        public string Key { get; set; }

        /// <inheritdoc/>
        [Required, MinLength(3), MaxLength(64)]
        public string Name { get; set; }

        /// <inheritdoc/>
        public string Value { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset CreatedAt { get; set; }

        /// <inheritdoc/>
        [CustomValidation(typeof(TokenValidation), nameof(TokenValidation.ExpirationValidation))]
        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddDays(90);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class TokenValidation
    {
        public static ValidationResult ExpirationValidation(DateTimeOffset expiresAt, ValidationContext _)
        {
            if (expiresAt < DateTimeOffset.UtcNow.AddMinutes(1))
                return new ValidationResult("ExpiresAt cannot be in the past.");
            else if (expiresAt.Year > 2050)
                return new ValidationResult("ExpiresAt cannot be later than 2050.");

            return default;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
