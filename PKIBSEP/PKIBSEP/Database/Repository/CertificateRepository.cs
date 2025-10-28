using Microsoft.EntityFrameworkCore;
using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Models.Certificate;

namespace PKIBSEP.Database.Repository
{
    /// <summary>
    /// EF Core repository for certificates and their CA keystore metadata.
    /// </summary>
    public sealed class CertificateRepository : ICertificateRepository
    {
        private readonly ApplicationDbContext _db;

        public CertificateRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<Certificate?> GetByIdAsync(int id)
        {
            return _db.Certificates
                      .AsNoTracking()
                      .FirstOrDefaultAsync(c => c.Id == id);
        }

        public Task<Certificate?> GetBySerialHexAsync(string serialHex)
        {
            return _db.Certificates
                      .AsNoTracking()
                      .FirstOrDefaultAsync(c => c.SerialHex == serialHex);
        }

        public async Task<int> InsertAsync(Certificate cert)
        {
            _db.Certificates.Add(cert);
            await _db.SaveChangesAsync();
            return cert.Id;
        }

        public async Task UpdateAsync(Certificate cert)
        {
            // If the entity is detached, Update will attach and mark modified.
            _db.Certificates.Update(cert);
            await _db.SaveChangesAsync();
        }

        /// <summary>After inserting a root, set ChainRootId = root id.</summary>
        public async Task SetChainRootAsync(int certificateId, int chainRootId)
        {
            var cert = await _db.Certificates.FirstOrDefaultAsync(c => c.Id == certificateId)
                       ?? throw new KeyNotFoundException($"Certificate {certificateId} not found.");

            cert.ChainRootId = chainRootId;
            await _db.SaveChangesAsync();
        }

        /// <summary>Fetch keystore record for given CA certificate.</summary>
        public Task<CaKeyMaterial?> GetCaKeyMaterialAsync(int certificateId)
        {
            // Unique index on CertificateId ensures at most one record.
            return _db.CaKeyMaterials
                      .AsNoTracking()
                      .FirstOrDefaultAsync(k => k.CertificateId == certificateId);
        }

        public async Task InsertCaKeyMaterialAsync(CaKeyMaterial material)
        {
            _db.CaKeyMaterials.Add(material);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Certificate>> GetAllAsync()
        {
            return await _db.Certificates.AsNoTracking().ToListAsync();
        }
    }
}
