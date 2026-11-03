using System.ComponentModel.DataAnnotations;

namespace CourtBooker.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Imię i nazwisko jest wymagane.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail jest wymagany.")]
    [EmailAddress(ErrorMessage = "Niepoprawny adres e-mail.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane.")]
    [DataType(DataType.Password)]
    [PasswordPolicy]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potwierdź hasło.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Hasła nie są identyczne.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
