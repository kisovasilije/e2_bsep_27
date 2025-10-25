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
    private readonly JwtOptions _jwtOptions;

    public AuthService(IUserRepository userRepository, IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthenticationResponseDto> LoginAsync(UserCredentialsDto userCredentials)
    {
        var user = await _userRepository.GetByEmailAsync(userCredentials.Email);
        if (user == null)
            throw new KeyNotFoundException($"User with email '{userCredentials.Email}' not found");

        if (!user.VerifyPassword(userCredentials.Password))
            throw new UnauthorizedAccessException("Invalid password");

        return new AuthenticationResponseDto
        {
            Email = user.Email,
            AccessToken = GenerateAccessToken(user)
        };
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
            expires: DateTime.UtcNow.AddHours(6),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}