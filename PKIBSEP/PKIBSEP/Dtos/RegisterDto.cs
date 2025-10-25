using System.ComponentModel.DataAnnotations;

namespace PKIBSEP.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email je obavezan")]
        [EmailAddress(ErrorMessage = "Neispravna email adresa")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna")]
        [MinLength(8, ErrorMessage = "Lozinka mora imati najmanje 8 karaktera")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrda lozinke je obavezna")]
        [Compare("Password", ErrorMessage = "Lozinke se ne poklapaju")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username je obavezan")]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Organizacija je obavezna")]
        [MaxLength(255)]
        public string Organization { get; set; } = string.Empty;
    }
}
