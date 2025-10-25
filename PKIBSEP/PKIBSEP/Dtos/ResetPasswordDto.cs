using System.ComponentModel.DataAnnotations;

namespace PKIBSEP.Dtos
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Token je obavezan")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nova lozinka je obavezna")]
        [MinLength(8, ErrorMessage = "Lozinka mora imati najmanje 8 karaktera")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrda lozinke je obavezna")]
        [Compare("NewPassword", ErrorMessage = "Lozinke se ne poklapaju")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
