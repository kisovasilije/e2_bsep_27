using Pki.Domain;
using PKIBSEP.Models.Certificate;

namespace PKIBSEP.Interfaces.Repository
{
    public interface ICaUserKeyRepository
    {
        Task<CaUserKey?> GetByUserAsync(int caUserId, int keyVersion = 1);
        Task InsertAsync(CaUserKey key);
    }
}
