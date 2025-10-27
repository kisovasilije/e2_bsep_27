using System.ComponentModel.DataAnnotations;

namespace PKIBSEP.Dtos.PasswordManager
{
    public class SharePasswordDto
    {
        [Required(ErrorMessage = "ID korisnika je obavezan")]
        public int TargetUserId { get; set; }

        [Required(ErrorMessage = "Enkriptovana lozinka je obavezna")]
        public string EncryptedPasswordForTarget { get; set; } = string.Empty;
    }
}
