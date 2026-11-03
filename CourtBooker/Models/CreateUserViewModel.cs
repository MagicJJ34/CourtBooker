using System.ComponentModel.DataAnnotations;

namespace CourtBooker.Models;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Podaj imię i nazwisko.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Podaj adres e-mail.")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Podaj hasło.")]
    [DataType(DataType.Password)]
    [PasswordPolicy]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Potwierdź hasło.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Hasła nie są zgodne.")]
    public string ConfirmPassword { get; set; } = "";
}
