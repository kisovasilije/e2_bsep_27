namespace PKIBSEP.Dtos.PasswordManager
{
    public class SharedUserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime SharedAt { get; set; }
    }
}
