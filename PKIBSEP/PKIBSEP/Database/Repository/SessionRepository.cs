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
}
