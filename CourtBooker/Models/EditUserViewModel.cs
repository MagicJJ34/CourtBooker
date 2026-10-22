using System.ComponentModel.DataAnnotations;

namespace CourtBooker.Models;

public class EditUserViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Podaj imię i nazwisko.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Podaj adres e-mail.")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Wybierz rolę.")]
    public string Role { get; set; } = "User";
}
