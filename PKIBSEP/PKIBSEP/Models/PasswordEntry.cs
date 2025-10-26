using System.ComponentModel.DataAnnotations;

namespace PKIBSEP.Models
{
    public class PasswordEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string SiteName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public int OwnerId { get; set; }

        public User Owner { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<PasswordShare> Shares { get; set; } = new List<PasswordShare>();
    }
}
