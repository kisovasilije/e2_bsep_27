using Microsoft.EntityFrameworkCore;
using PKIBSEP.Interfaces.Repository;

namespace PKIBSEP.Database.Repository
{
    /// <summary>
    /// EF Core access for CA chain assignments (which chain roots a CA user may use).
    /// </summary>
    public sealed class CaAssignmentRepository : ICaAssignmentRepository
    {
        private readonly ApplicationDbContext _db;

        public CaAssignmentRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// True if there exists an active assignment for (caUserId, chainRootCertificateId).
        /// </summary>
        public Task<bool> IsChainAssignedToUserAsync(int caUserId, int chainRootCertificateId)
        {
            return _db.CaAssignments
                      .AsNoTracking()
                      .AnyAsync(a =>
                          a.CaUserId == caUserId &&
                          a.ChainRootCertificateId == chainRootCertificateId &&
                          a.IsActive);
        }

        /// <summary>
        /// Creates a new CA assignment, linking a CA user to a certificate chain.
        /// </summary>
        public async Task<int> InsertAsync(Models.Certificate.CaAssignment assignment)
        {
            _db.CaAssignments.Add(assignment);
            await _db.SaveChangesAsync();
            return assignment.Id;
        }

        /// <summary>
        /// Gets all active chain root certificate IDs assigned to a specific CA user.
        /// </summary>
        public Task<List<int>> GetAssignedChainRootsForUserAsync(int caUserId)
        {
            return _db.CaAssignments
                      .AsNoTracking()
                      .Where(a => a.CaUserId == caUserId && a.IsActive)
                      .Select(a => a.ChainRootCertificateId)
                      .Distinct()
                      .ToListAsync();
        }
    }
}
