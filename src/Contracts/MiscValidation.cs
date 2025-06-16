using System.ComponentModel.DataAnnotations;

namespace TcpWtf.NumberSequence.Contracts
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class MiscValidation
    {
        public static ValidationResult DateOnlyWithinTenYears(DateOnly value, ValidationContext _)
        {
            if (value == default)
            {
                return default;
            }

            if (value < DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddYears(-10).UtcDateTime))
            {
                return new ValidationResult("LastOccurrence cannot be older than 10 years from now.");
            }
            else if (value > DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddYears(10).UtcDateTime))
            {
                return new ValidationResult("LastOccurrence cannot be later than 10 years from now.");
            }

            return default;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
