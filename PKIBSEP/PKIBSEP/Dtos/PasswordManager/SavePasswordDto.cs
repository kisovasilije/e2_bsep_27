using System.ComponentModel.DataAnnotations;

namespace PKIBSEP.Dtos.PasswordManager
{
    public class SavePasswordDto
    {
        [Required(ErrorMessage = "Naziv sajta je obavezan")]
        [MaxLength(255)]
        public string SiteName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Korisničko ime je obavezno")]
        [MaxLength(255)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enkriptovana lozinka je obavezna")]
        public string EncryptedPassword { get; set; } = string.Empty;
    }
}
