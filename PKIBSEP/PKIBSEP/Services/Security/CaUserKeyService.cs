using Microsoft.AspNetCore.DataProtection;
using Pki.Domain;
using PKIBSEP.Interfaces;
using PKIBSEP.Interfaces.Repository;

namespace PKIBSEP.Services.Security
{
    /// <summary>
    /// Manages per-CA-user wrap keys (32-byte symmetric keys) used to encrypt PFX passwords.
    /// Wrap keys are themselves protected by ASP.NET Data Protection.
    /// </summary>
    public class CaUserKeyService: ICaUserKeyService
    {
        private readonly IDataProtector _dataProtector;
        private readonly ICaUserKeyRepository _caUserKeyRepository;

        public CaUserKeyService(IDataProtectionProvider dataProtectionProvider, ICaUserKeyRepository caUserKeyRepository)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("PKI.CAUserWrapKey");
            _caUserKeyRepository = caUserKeyRepository;
        }

        /// <summary>
        /// Returns the per-CA-user 32-byte wrap key. Creates and persists it if it doesn't exist.
        /// </summary>
        public async Task<byte[]> GetOrCreateWrapKeyAsync(int caUserId)
        {
            var existing = await _caUserKeyRepository.GetByUserAsync(caUserId);
            if (existing != null)
            {
                var protectedBytes = Convert.FromBase64String(existing.ProtectedWrapKey);
                return _dataProtector.Unprotect(protectedBytes);
            }

            var newWrapKey = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            var protectedWrapKey = _dataProtector.Protect(newWrapKey);

            await _caUserKeyRepository.InsertAsync(new CaUserKey
            {
                CaUserId = caUserId,
                ProtectedWrapKey = Convert.ToBase64String(protectedWrapKey),
                KeyVersion = 1,
                CreatedAtUtc = DateTime.UtcNow
            });

            return newWrapKey;
        }
    }
}
