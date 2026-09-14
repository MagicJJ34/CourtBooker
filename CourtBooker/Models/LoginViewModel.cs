using System.ComponentModel.DataAnnotations;

namespace CourtBooker.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Pole Email jest wymagane.")]
    [EmailAddress(ErrorMessage = "Niepoprawny format adresu email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pole Hasło jest wymagane.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}