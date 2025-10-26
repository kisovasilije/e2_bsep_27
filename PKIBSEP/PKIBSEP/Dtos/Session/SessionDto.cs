namespace PKIBSEP.Dtos.Session;

public record SessionDto(
    int Id,
    int UserId,
    string IpAddress,
    string UserAgent,
    DateTime LastActive);
