namespace PKIBSEP.Dtos
{
    public class RegularUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool HasPublicKey { get; set; }
    }
}
