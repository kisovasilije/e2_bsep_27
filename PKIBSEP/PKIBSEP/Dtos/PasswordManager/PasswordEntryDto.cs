namespace PKIBSEP.Dtos.PasswordManager
{
    public class PasswordEntryDto
    {
        public int Id { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string EncryptedPassword { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
        public string OwnerEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
