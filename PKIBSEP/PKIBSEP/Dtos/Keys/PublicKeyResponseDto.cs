namespace PKIBSEP.Dtos.Keys
{
    public class PublicKeyResponseDto
    {
        public string? PublicKeyPem { get; set; }
        public DateTime? KeyGeneratedAt { get; set; }
        public bool HasKey { get; set; }
    }
}
