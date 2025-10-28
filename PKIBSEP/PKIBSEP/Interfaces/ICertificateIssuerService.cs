using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.X509;
using PKIBSEP.Models.Certificate;

namespace PKIBSEP.Interfaces
{
    /// <summary>
    /// Facade for centralized CA certificate issuance.
    /// Exposes operations to issue Root and Intermediate CA certificates.
    /// </summary>
    public interface ICertificateIssuerService
    {
        /// <summary>
        /// Creates a self-signed Root CA certificate and persists:
        /// - certificate metadata (including PEM + chain),
        /// - keystore (PFX with private key) protected via per-CA-user wrap key,
        /// - CA assignment (linking the target CA user to this chain root).
        /// </summary>
        /// <param name="subject">Subject DN (X.500 name) for the Root CA.</param>
        /// <param name="validityDays">Validity period in days.</param>
        /// <param name="pathLen">
        /// BasicConstraints pathLenConstraint for the Root.
        /// null = unlimited, 0 = may only issue EE, N&gt;0 = allowed CA depth below.
        /// </param>
        /// <param name="keyUsageFlags">BouncyCastle KeyUsage flags (e.g., KeyCertSign | CrlSign).</param>
        /// <param name="targetCaUserId">CA user id who will own this root's keystore.</param>
        /// <param name="assignedByUserId">User ID who is creating this certificate (typically Admin).</param>
        /// <returns>Persisted <see cref="Certificate"/> record.</returns>
        Task<Certificate> CreateRootAsync(X509Name subject, int validityDays, int? pathLen, int keyUsageFlags, int targetCaUserId, int assignedByUserId);

        /// <summary>
        /// Issues an Intermediate CA certificate under the given issuer CA and persists:
        /// - certificate metadata (including PEM + full chain),
        /// - keystore (PFX with private key) protected via per-CA-user wrap key,
        /// - CA assignment (linking the target CA user to this chain root if needed).
        /// </summary>
        /// <param name="issuerCertificateId">DB id of the issuer CA certificate.</param>
        /// <param name="subject">Subject DN (X.500 name) for the new Intermediate CA.</param>
        /// <param name="validityDays">Requested validity in days (clamped to issuer's NotAfter).</param>
        /// <param name="pathLen">
        /// BasicConstraints pathLenConstraint for the Intermediate.
        /// null = unlimited, 0 = may only issue EE, N&gt;0 = allowed CA depth below.
        /// </param>
        /// <param name="keyUsageFlags">BouncyCastle KeyUsage flags (e.g., KeyCertSign | CrlSign).</param>
        /// <param name="targetCaUserId">CA user id who will own this intermediate's keystore.</param>
        /// <param name="assignedByUserId">User ID who is creating this certificate (typically Admin or the CA user themselves).</param>
        /// <returns>Persisted <see cref="Certificate"/> record.</returns>
        Task<Certificate> IssueIntermediateAsync(int issuerCertificateId, X509Name subject, int validityDays, int? pathLen, int keyUsageFlags, int targetCaUserId, int assignedByUserId, bool isAdmin);

        /// <summary>
        /// Vraća sve CA sertifikate (za Admin)
        /// </summary>
        Task<List<Certificate>> GetAllCACertificatesAsync();

        /// <summary>
        /// Vraća CA sertifikate dostupne određenom CA korisniku (samo iz dodeljenih lanaca)
        /// </summary>
        Task<List<Certificate>> GetUserCACertificatesAsync(int caUserId);
    }
}
