using FluentResults;
using PKIBSEP.Common;
using PKIBSEP.Dtos;
using PKIBSEP.Interfaces;
using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Models;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PKIBSEP.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository sessionRepository;

    public SessionService(ISessionRepository sessionRepository)
    {
        this.sessionRepository = sessionRepository;
    }

    public async Task<Result> CreateAsync(AuthenticationDto auth)
    {
        using var sha = SHA256.Create();
        var jwtHash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(auth.AccessToken));

        var session = new Session(auth.UserId, jwtHash, JwtOptions.DefaultExpiresAt, auth.IpAddress, auth.UserAgent);

        try
        {
            await sessionRepository.CreateAsync(session);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
