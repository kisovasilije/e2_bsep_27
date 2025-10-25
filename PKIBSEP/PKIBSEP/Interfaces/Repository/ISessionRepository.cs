using PKIBSEP.Models;

namespace PKIBSEP.Interfaces.Repository;

public interface ISessionRepository
{
    Task<Session> CreateAsync(Session session);
}
