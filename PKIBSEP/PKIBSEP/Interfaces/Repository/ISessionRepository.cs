using PKIBSEP.Models;

namespace PKIBSEP.Interfaces.Repository;

public interface ISessionRepository
{
    Task<Session> CreateAsync(Session session);

    /// <summary>
    /// Gets all sessions for a specific user by user ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<IEnumerable<Session>> GetByUserIdAsync(int userId);

    Task<bool> RevokeCurrentSessionAsync(byte[] jwtHash);
}
