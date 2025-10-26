namespace PKIBSEP.Dtos.Keys
{
    public class UserPublicKeyDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PublicKeyPem { get; set; }
        public bool HasKey { get; set; }
    }
}
