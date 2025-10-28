using Microsoft.EntityFrameworkCore;
using Pki.Domain;
using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Models.Certificate;

namespace PKIBSEP.Database.Repository
{
    /// <summary>
    /// Stores and retrieves the per-CA-user protected wrap key used to encrypt keystore (PFX) passwords.
    /// </summary>
    public sealed class CaUserKeyRepository : ICaUserKeyRepository
    {
        private readonly ApplicationDbContext _db;

        public CaUserKeyRepository(ApplicationDbContext db) => _db = db;

        /// <summary>
        /// Returns the protected wrap key for a given CA user (and key version).
        /// Your model has a unique index on (CaUserId, KeyVersion), so this returns at most one record.
        /// </summary>
        public Task<CaUserKey?> GetByUserAsync(int caUserId, int keyVersion = 1)
        {
            return _db.CaUserKeys
                      .AsNoTracking()
                      .FirstOrDefaultAsync(x => x.CaUserId == caUserId && x.KeyVersion == keyVersion);
        }

        /// <summary>
        /// Inserts a new per-user wrap key record.
        /// </summary>
        public async Task InsertAsync(CaUserKey key)
        {
            _db.CaUserKeys.Add(key);
            await _db.SaveChangesAsync();
        }
    }
}
