using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CourtBooker.Models;

public class PasswordPolicyAttribute : ValidationAttribute
{
    public PasswordPolicyAttribute()
    {
        ErrorMessage = "Hasło musi mieć minimum 10 znaków, wielką i małą literę, cyfrę oraz znak specjalny.";
    }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string password)
            return new ValidationResult(ErrorMessage);

        if (string.IsNullOrEmpty(password))
            return ValidationResult.Success;

        bool valid =
            password.Length >= 10 &&
            Regex.IsMatch(password, @"[A-Z]") &&
            Regex.IsMatch(password, @"[a-z]") &&
            Regex.IsMatch(password, @"[0-9]") &&
            Regex.IsMatch(password, @"[^a-zA-Z0-9]");

        return valid
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage);
    }
}
