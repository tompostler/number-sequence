using System.ComponentModel.DataAnnotations;

namespace number_sequence.Pages.UI.Chiro
{
    /// <summary>
    /// Rejects a date of service more than a year old or more than a day ahead.
    /// A parse without a date leaves <c>0001-01-01</c> since DateOnly is never null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PlausibleDateOfServiceAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not DateOnly date)
            {
                return ValidationResult.Success;
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            if (date < today.AddYears(-1) || date > today.AddDays(1))
            {
                return new ValidationResult($"Date of service {date:yyyy-MM-dd} must be within the last year and not in the future.", [validationContext.MemberName]);
            }

            return ValidationResult.Success;
        }
    }
}
