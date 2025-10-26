using Microsoft.IdentityModel.Tokens;
using PKIBSEP.Dtos;
using PKIBSEP.Interfaces;
using PKIBSEP.Models;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PKIBSEP.Common;
using Microsoft.Extensions.Options;

namespace PKIBSEP.Services;
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionService _sessionService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(IUserRepository userRepository, ISessionService sessionService, IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _sessionService = sessionService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthenticationResponseDto> LoginAsync(AuthenticationDto auth)
    {
        if (!await VerifyRecaptchaAsync(auth.CaptchaToken))
        {
            throw new UnauthorizedAccessException("Invalid captcha");
        }

        var user = await _userRepository.GetByEmailAsync(auth.Email);
        if (user == null)
            throw new KeyNotFoundException($"User with email '{auth.Email}' not found");

        if (!user.VerifyPassword(auth.Password))
            throw new UnauthorizedAccessException("Invalid password");

        var accessToken = GenerateAccessToken(user);
        auth = auth with { UserId = user.Id, AccessToken = accessToken };
        var result = await _sessionService.CreateAsync(auth);
        if (result.IsFailed)
        {
            throw new Exception("Failed to create session: " + string.Join(", ", result.Errors.Select(e => e.Message)));
        }

        return new AuthenticationResponseDto
        {
            Email = user.Email,
            AccessToken = accessToken
        };
    }

    private async Task<bool> VerifyRecaptchaAsync(string captchaToken)
    {
        using var client = new HttpClient();
        var values = new Dictionary<string, string>
        {
            { "secret", _jwtOptions.RecaptchaSecret },
            { "response", captchaToken }
        };

        var content = new FormUrlEncodedContent(values);
        var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
        var json = await response.Content.ReadAsStringAsync();

        var result = System.Text.Json.JsonSerializer.Deserialize<RecaptchaResponse>(json);
        return result?.Success ?? false;
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("role", user.Role.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: JwtOptions.DefaultExpiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}