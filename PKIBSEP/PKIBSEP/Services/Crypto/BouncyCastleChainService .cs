using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Interfaces;
using Org.BouncyCastle.X509;
using System.Text;

namespace PKIBSEP.Services.Crypto
{
    /// <summary>
    /// Verifies that an issuer's certificate chain is cryptographically valid:
    /// - each child cert is signed by its parent,
    /// - the root at the top is self-signed.
    /// </summary>
    public class BouncyCastleChainService : IChainService
    {
        private readonly ICertificateRepository _certificateRepository;

        public BouncyCastleChainService(ICertificateRepository certificateRepository)
        {
            _certificateRepository = certificateRepository;
        }

        /// <summary>
        /// Builds the issuer's chain (issuer -> ... -> root) from the database and verifies signatures.
        /// Returns true only if the chain is fully valid.
        /// </summary>
        public async Task<bool> VerifyIssuerChainAsync(int issuerCertificateId)
        {
            var certParser = new X509CertificateParser();

            // Will contain the chain as BouncyCastle X509Certificate objects, in order:
            // [issuer/child, parent, ..., root]
            var chainCertificates = new List<X509Certificate>();

            // Walk up the chain using DB parent links, starting from the issuer certificate
            var currentDbCertificate = await _certificateRepository.GetByIdAsync(issuerCertificateId);
            while (currentDbCertificate != null)
            {
                try
                {
                    var currentBcCertificate = certParser.ReadCertificate(
                        Encoding.ASCII.GetBytes(currentDbCertificate.PemCertificate));
                    chainCertificates.Add(currentBcCertificate);
                }
                catch
                {
                    // PEM parse failed → chain invalid
                    return false;
                }

                // Move to the parent (null means we've reached the root)
                currentDbCertificate = currentDbCertificate.ParentCertificateId is int parentId
                    ? await _certificateRepository.GetByIdAsync(parentId)
                    : null;
            }

            if (chainCertificates.Count == 0)
                return false; // nothing loaded → invalid

            // Verify each hop: child's signature must validate with parent's public key
            for (int i = 0; i < chainCertificates.Count - 1; i++)
            {
                var childCert = chainCertificates[i];
                var parentCert = chainCertificates[i + 1];

                try
                {
                    childCert.Verify(parentCert.GetPublicKey());
                }
                catch
                {
                    // Signature mismatch (wrong parent or tampered certificate)
                    return false;
                }
            }

            // Finally, verify the root is self-signed
            var rootCert = chainCertificates[^1];
            try
            {
                rootCert.Verify(rootCert.GetPublicKey());
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
