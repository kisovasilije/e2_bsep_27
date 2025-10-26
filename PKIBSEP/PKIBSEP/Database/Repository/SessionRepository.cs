using Microsoft.EntityFrameworkCore;
using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Models;

namespace PKIBSEP.Database.Repository;

public class SessionRepository : ISessionRepository
{
    private readonly ApplicationDbContext context;

    private readonly DbSet<Session> sessions;

    public SessionRepository(ApplicationDbContext context)
    {
        this.context = context;
        sessions = context.Set<Session>();
    }

    public async Task<Session> CreateAsync(Session session)
    {
        sessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }

    public async Task<IEnumerable<Session>> GetByUserIdAsync(int userId)
    {
        return await sessions.Where(s => s.UserId == userId && !s.IsRevoked).ToListAsync();
    }

    public async Task<bool> RevokeCurrentSessionAsync(byte[] jwtHash)
    {
        try
        {
            var session = await sessions.FirstOrDefaultAsync(s => s.JwtHash.SequenceEqual(jwtHash) && !s.IsRevoked);
            if (session is null)
            {
                return false;
            }

            session.Revoke();
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<Session?> GetByJwtHashAsync(byte[] jwtHash)
    {
        return await sessions.FirstOrDefaultAsync(s => s.JwtHash.SequenceEqual(jwtHash));
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
