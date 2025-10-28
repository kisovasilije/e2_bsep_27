using PKIBSEP.Models.Certificate;

namespace PKIBSEP.Interfaces.Repository
{
    public interface ICertificateRepository
    {
        Task<Certificate?> GetByIdAsync(int id);
        Task<Certificate?> GetBySerialHexAsync(string serialHex);

        Task<int> InsertAsync(Certificate cert);
        Task UpdateAsync(Certificate cert);

        /// <summary>After inserting a root, set ChainRootId = root id.</summary>
        Task SetChainRootAsync(int certificateId, int chainRootId);

        /// <summary>Fetch keystore record for given CA certificate.</summary>
        Task<CaKeyMaterial?> GetCaKeyMaterialAsync(int certificateId);

        Task InsertCaKeyMaterialAsync(CaKeyMaterial material);
    }
}
