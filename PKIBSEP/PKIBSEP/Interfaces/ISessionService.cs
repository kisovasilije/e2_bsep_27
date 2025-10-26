using FluentResults;
using PKIBSEP.Dtos;
using PKIBSEP.Dtos.Session;

namespace PKIBSEP.Interfaces;

public interface ISessionService
{
    Task<Result> CreateAsync(AuthenticationDto auth);

    /// <summary>
    /// Gets all sessions for a specific user by user ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<Result<IEnumerable<SessionDto>>> GetByUserIdAsync(int userId, string token);

    Task<Result> RevokeCurrentSessionAsync(string token);

    Task<Result> ValidateSessionAsync(string token);

    Task<Result> RevokeByIdAsync(int id);

    Task<Result> RevokeAllAsync(int userId, string token);
}
