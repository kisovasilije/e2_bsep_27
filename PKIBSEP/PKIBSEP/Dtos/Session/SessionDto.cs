namespace PKIBSEP.Dtos.Session;

public class SessionDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string IpAddress { get; set; }

    public string UserAgent { get; set; }

    public bool IsThisSession { get; set; }

    public DateTime LastActive { get; set; }
}
