using PKIBSEP.Interfaces;
using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Models.Certificate;
using PKIBSEP.Security;
using PKIBSEP.Services.Security;
using System.Security.Cryptography.X509Certificates;

namespace PKIBSEP.Services.Keystore
{
    /// <summary>
    /// Handles CA keystore (.pfx) persistence and password protection.
    /// - Exports PFX (private key + chain) with a random password.
    /// - Encrypts that password using the CA user's wrap key.
    /// - Stores file path and encrypted password in the database.
    /// </summary>
    public class KeystoreService:IKeystoreService
    {
        private readonly ICaUserKeyService _caUserKeyService;
        private readonly ICertificateRepository _certificateRepository;
        private readonly string _keystoreFolderPath;

        public KeystoreService(
            ICaUserKeyService caUserKeyService,
            ICertificateRepository certificateRepository,
            IConfiguration configuration)
        {
            _caUserKeyService = caUserKeyService;
            _certificateRepository = certificateRepository;

            _keystoreFolderPath = configuration["Keystore:Folder"]
                ?? throw new InvalidOperationException("Keystore:Folder is not configured.");
        }

        /// <summary>
        /// Saves a CA PFX (private key + chain) to disk and stores its password encrypted with the per-user wrap key.
        /// </summary>
        /// <param name="caUserId">The CA user who owns/operates this CA keystore.</param>
        /// <param name="certificateId">The database id of the CA certificate.</param>
        /// <param name="certificateWithPrivateKey">X509Certificate2 that contains the private key for the CA cert.</param>
        /// <param name="issuerChain">Optional chain (X509Certificate2Collection) to include in the PFX.</param>
        public async Task SaveCaPfxAsync(
            int caUserId,
            int certificateId,
            X509Certificate2 certificateWithPrivateKey,
            X509Certificate2Collection? issuerChain = null)
        {
            // 1) Generate a strong random password for the PFX
            var pfxPassword = PasswordGenerator.Random(24);

            // 2) Build collection: certificate (already has private key) + optional chain
            //    NOTE: certificateWithPrivateKey already contains the private key (attached in BcToX509WithPrivateKey)
            var collection = new X509Certificate2Collection();
            collection.Add(certificateWithPrivateKey); // Already has the private key attached

            // Add chain certificates (without private keys)
            if (issuerChain != null && issuerChain.Count > 0)
            {
                collection.AddRange(issuerChain);
            }

            // 3) Export the entire collection as PFX (includes the private key from certificateWithPrivateKey)
            var pfxBytes = collection.Export(X509ContentType.Pkcs12, pfxPassword);

            var fileName = $"{certificateWithPrivateKey.Thumbprint}.pfx";
            var fullPath = Path.Combine(_keystoreFolderPath, fileName);
            await File.WriteAllBytesAsync(fullPath, pfxBytes);

            // 3) Encrypt the PFX password using the CA user's wrap key
            var wrapKey = await _caUserKeyService.GetOrCreateWrapKeyAsync(caUserId);
            var encryptedPfxPassword = AesGcmHelper.EncryptToBase64(wrapKey, pfxPassword);

            // 4) Persist keystore metadata
            await _certificateRepository.InsertCaKeyMaterialAsync(new CaKeyMaterial
            {
                CertificateId = certificateId,
                OwnerId = caUserId, // Store the owner (whose wrap key encrypts the password)
                PfxFilePath = fullPath,
                EncryptedPfxPassword = encryptedPfxPassword,
                IsActive = true,
                KeystoreAlias = certificateWithPrivateKey.Thumbprint // acts as an alias
            });
        }

        /// <summary>
        /// Loads the issuer's PFX, decrypts the password, and returns the unlocked cert for signing.
        /// </summary>
        public async Task<(X509Certificate2 IssuerCertificate, string PfxPassword)> LoadIssuerAsync(int caUserId, CaKeyMaterial caKeyMaterial)
        {
            // 1) Decrypt the PFX password using the CA user's wrap key
            var wrapKey = await _caUserKeyService.GetOrCreateWrapKeyAsync(caUserId);
            var pfxPassword = AesGcmHelper.DecryptFromBase64(wrapKey, caKeyMaterial.EncryptedPfxPassword);

            // 2) Load the PFX into an X509Certificate2 instance that allows private key export (for BC interop)
            var issuerCert = new X509Certificate2(
                File.ReadAllBytes(caKeyMaterial.PfxFilePath),
                pfxPassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

            return (issuerCert, pfxPassword);
        }
    }
}
