using PKIBSEP.Enums;
using System.ComponentModel.DataAnnotations;

namespace PKIBSEP.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Organization { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.RegularUser;

        public bool IsEmailConfirmed { get; set; } = false;

        public string? EmailConfirmationToken { get; set; }

        public DateTime? EmailConfirmationTokenExpiry { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiry { get; set; }

        public bool MustChangePassword { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }
    }
}
