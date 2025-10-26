using System.ComponentModel.DataAnnotations;

namespace PKIBSEP.Dtos.PasswordManager
{
    public class UpdatePasswordDto
    {
        [Required(ErrorMessage = "ID je obavezan")]
        public int Id { get; set; }

        [MaxLength(255)]
        public string? SiteName { get; set; }

        [MaxLength(255)]
        public string? Username { get; set; }

        public string? EncryptedPassword { get; set; }
    }
}
