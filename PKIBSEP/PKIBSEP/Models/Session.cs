namespace PKIBSEP.Models;

public class Session
{
    public int Id { get; init; }

    public int UserId { get; init; }

    public byte[] JwtHash { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public bool IsRevoked { get; private set; } = false;

    public string IpAddress { get; private set; }

    public string UserAgent { get; private set; }

    public DateTime LastActive { get; private set; } = DateTime.UtcNow;

    public User? User { get; private set; }

    public Session(int userId, byte[] jwtHash, DateTime expiresAt, string ipAddress, string userAgent)
    {
        UserId = userId;
        JwtHash = jwtHash;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}
