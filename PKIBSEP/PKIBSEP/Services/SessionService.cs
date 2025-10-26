using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using PKIBSEP.Common;
using PKIBSEP.Dtos;
using PKIBSEP.Dtos.Session;
using PKIBSEP.Interfaces;
using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Models;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PKIBSEP.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository sessionRepository;

    private readonly IMapper mapper;

    public SessionService(ISessionRepository sessionRepository, IMapper mapper)
    {
        this.sessionRepository = sessionRepository;
        this.mapper = mapper;
    }

    private static byte[] GetJwtHash(string jwt)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(jwt));
    }

    public async Task<Result> CreateAsync(AuthenticationDto auth)
    {
        var session = new Session(auth.UserId, GetJwtHash(auth.AccessToken), JwtOptions.DefaultExpiresAt, auth.IpAddress, auth.UserAgent);

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

    public async Task<Result<IEnumerable<SessionDto>>> GetByUserIdAsync(int userId, string token)
    {
        IEnumerable<SessionDto> response;
        try
        {
            var sessions = await sessionRepository.GetByUserIdAsync(userId);
            var current = sessions.FirstOrDefault(s => s.JwtHash.SequenceEqual(GetJwtHash(token)));
            response = mapper.Map<IEnumerable<SessionDto>>(sessions);
            var currentResponse = response.FirstOrDefault(s => s.Id == current?.Id);
            currentResponse.IsThisSession = true;
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
        
        return Result.Ok(response);
    }

    public async Task<Result> RevokeCurrentSessionAsync(string token)
    {
        if (await sessionRepository.RevokeCurrentSessionAsync(GetJwtHash(token)))
        {
            return Result.Ok();
        }
        else
        {
            return Result.Fail("Failed to revoke session.");
        }
    }

    public async Task<Result> ValidateSessionAsync(string token)
    {
        var session = await sessionRepository.GetByJwtHashAsync(GetJwtHash(token));
        if (session is null)
        {
            return Result.Fail("Session not found.");
        }

        if (session.IsRevoked)
        {
            return Result.Fail("Session is revoked.");
        }

        if (session.ShouldUpdateLastActive())
        {
            session.UpdateLastActive();
            await sessionRepository.SaveChangesAsync();
        }

        return Result.Ok();
    }

    public async Task<Result> RevokeByIdAsync(int id)
    {
        var session = await sessionRepository.GetByIdAsync(id);
        if (session is null)
        {
            return Result.Fail("Session does not exist.");
        }

        session.Revoke();
        await sessionRepository.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> RevokeAllAsync(int userId, string token)
    {
        var sessions = await sessionRepository.GetByUserIdAsync(userId);
        foreach (var session in sessions)
        {
            if (session.JwtHash.SequenceEqual(GetJwtHash(token)))
            {
                continue;
            }
            session.Revoke();
        }

        await sessionRepository.SaveChangesAsync();
        return Result.Ok();
    }
}
