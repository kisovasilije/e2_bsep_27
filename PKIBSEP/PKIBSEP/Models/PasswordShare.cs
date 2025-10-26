using System.ComponentModel.DataAnnotations;

namespace PKIBSEP.Models
{
    public class PasswordShare
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PasswordEntryId { get; set; }

        public PasswordEntry PasswordEntry { get; set; } = null!;

        [Required]
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        [Required]
        public string EncryptedPassword { get; set; } = string.Empty;

        public DateTime SharedAt { get; set; } = DateTime.UtcNow;
    }
}
