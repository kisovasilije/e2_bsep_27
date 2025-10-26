namespace PKIBSEP.Dtos;

public record AuthenticationDto (
    int UserId,
    string Email,
    string Password,
    string CaptchaToken,
    string IpAddress,
    string UserAgent,
    string AccessToken)
{
    public AuthenticationDto(string email, string password, string captchaToken, string ipAddress, string userAgent)
        : this(0, email, password, captchaToken, ipAddress, userAgent, string.Empty)
    {
    }
}
