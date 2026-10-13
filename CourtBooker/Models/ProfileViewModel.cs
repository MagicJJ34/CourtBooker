
using System.ComponentModel.DataAnnotations;

namespace CourtBooker.Models;

public class ProfileViewModel
{
    [Required(ErrorMessage = "Podaj imię i nazwisko.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Podaj adres e-mail.")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy e-mail.")]
    public string Email { get; set; } = "";

    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Hasło musi mieć minimum 6 znaków.")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Hasła nie są zgodne.")]
    public string? ConfirmPassword { get; set; }
}
