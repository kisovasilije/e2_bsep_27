using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Interfaces;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using PKIBSEP.Models.Certificate;
using PKIBSEP.Common.Enum;

namespace PKIBSEP.Services.Issuance
{
    /// <summary>
    /// Orchestrates issuance of CA certificates (Root and Intermediate):
    /// - builds/validates inputs
    /// - enforces issuer validity (time, CA flag, pathLen, chain signature, revocation)
    /// - persists cert metadata and chain
    /// - stores CA private key + chain into a protected PFX via KeystoreService
    /// </summary>
    public sealed class CertificateIssuerService : ICertificateIssuerService
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IIssuerValidationService _issuerValidationService;
        private readonly IKeystoreService _keystoreService;
        private readonly ICertificateGenerator _certificateGenerator;
        private readonly ICaAssignmentRepository _caAssignmentRepository;
        private readonly IUserRepository _userRepository;

        public CertificateIssuerService(
            ICertificateRepository certificateRepository,
            IIssuerValidationService issuerValidationService,
            IKeystoreService keystoreService,
            ICertificateGenerator certificateGenerator,
            ICaAssignmentRepository caAssignmentRepository,
            IUserRepository userRepository)
        {
            _certificateRepository = certificateRepository;
            _issuerValidationService = issuerValidationService;
            _keystoreService = keystoreService;
            _certificateGenerator = certificateGenerator;
            _caAssignmentRepository = caAssignmentRepository;
            _userRepository = userRepository;
        }

        // ---- Root issuance ----
        public async Task<Certificate> CreateRootAsync(X509Name subject, int validityDays, int? pathLen, int keyUsageFlags, int createdByAdminId)
        {
            var now = DateTime.UtcNow;
            var notBefore = now.AddMinutes(-5);
            var notAfter = now.AddDays(validityDays);

            var (keyPair, bcCert) = _certificateGenerator.CreateSelfSignedRoot(subject, notBefore, notAfter, pathLen, keyUsageFlags);
            var pem = ToPem(bcCert);

            var model = new Certificate
            {
                SerialHex = bcCert.SerialNumber.ToString(16),
                SubjectDistinguishedName = bcCert.SubjectDN.ToString(),
                IssuerDistinguishedName = bcCert.IssuerDN.ToString(),
                NotBeforeUtc = bcCert.NotBefore.ToUniversalTime(),
                NotAfterUtc = bcCert.NotAfter.ToUniversalTime(),
                IsCertificateAuthority = true,
                PathLenConstraint = pathLen,
                Type = CertificateType.RootCa,
                PemCertificate = pem,
                ChainPem = pem
            };

            var newId = await _certificateRepository.InsertAsync(model);
            await _certificateRepository.SetChainRootAsync(newId, newId); // root's ChainRootId = self

            // Save Root CA private key under Admin's wrap key (no CaAssignment created)
            var leafX509 = BcToX509WithPrivateKey(bcCert, keyPair);
            await _keystoreService.SaveCaPfxAsync(createdByAdminId, newId, leafX509, new X509Certificate2Collection());

            return (await _certificateRepository.GetByIdAsync(newId))!;
        }

        // ---- Intermediate issuance ----
        public async Task<Certificate> IssueIntermediateAsync(
            int issuerCertificateId, X509Name subject, int validityDays, int? pathLen, int keyUsageFlags, int targetCaUserId, int assignedByUserId, bool isAdmin)
        {
            // 1) Validate issuer (time window, CA flag, pathLen, chain signature, revocation)
            await _issuerValidationService.ValidateAsync(issuerCertificateId, true, pathLen);

            // 2) Get issuer certificate
            var issuer = await _certificateRepository.GetByIdAsync(issuerCertificateId)
                         ?? throw new Exception("ISSUER_NOT_FOUND");

            // 3) If CA User (not Admin), validate access to issuer chain and organization
            if (!isAdmin)
            {
                // SIGURNOST: CA korisnik NE SME da koristi root CA sertifikat kao issuer
                // Root CA privatni ključevi su dostupni samo administratoru
                if (issuer.Type == CertificateType.RootCa)
                    throw new UnauthorizedAccessException("CA_USER_CANNOT_USE_ROOT_CA_AS_ISSUER");

                // CA User must have access to the issuer chain they selected
                var issuerChainRootId = issuer.ChainRootId > 0 ? issuer.ChainRootId : issuer.Id;
                var hasAccess = await _caAssignmentRepository.IsChainAssignedToUserAsync(assignedByUserId, issuerChainRootId);
                if (!hasAccess)
                    throw new UnauthorizedAccessException("CA_USER_NO_ACCESS_TO_CHAIN");

                // If issuing to another CA User (not self), validate same organization
                if (targetCaUserId != assignedByUserId)
                {
                    var assignerUser = await _userRepository.GetByIdAsync(assignedByUserId)
                                       ?? throw new Exception("ASSIGNER_USER_NOT_FOUND");
                    var targetUser = await _userRepository.GetByIdAsync(targetCaUserId)
                                     ?? throw new Exception("TARGET_USER_NOT_FOUND");

                    if (assignerUser.Organization != targetUser.Organization)
                        throw new UnauthorizedAccessException("CANNOT_ISSUE_ACROSS_ORGANIZATIONS");
                }
            }

            // 4) Clamp validity to issuer.NotAfter

            var now = DateTime.UtcNow;
            var notBefore = now.AddMinutes(-5);
            var requestedNotAfter = now.AddDays(validityDays);
            var notAfter = requestedNotAfter <= issuer.NotAfterUtc ? requestedNotAfter : issuer.NotAfterUtc;

            // 5) Load issuer .pfx and extract private key for signing
            var caKeyMaterial = await _certificateRepository.GetCaKeyMaterialAsync(issuer.Id)
                                ?? throw new Exception("ISSUER_KEYSTORE_NOT_FOUND");

            // KRITIČNA SIGURNOSNA PROVERA:
            // CA korisnik može koristiti SAMO sertifikate čiji je on vlasnik privatnog ključa
            // Ovo sprečava CA korisnike da koriste sertifikate drugih CA korisnika iz istog lanca
            if (!isAdmin && caKeyMaterial.OwnerId != assignedByUserId)
                throw new UnauthorizedAccessException("CA_USER_NOT_OWNER_OF_ISSUER_PRIVATE_KEY");

            // Use the issuer keystore owner's wrap key to decrypt the PFX password
            var (issuerX509, _) = await _keystoreService.LoadIssuerAsync(caKeyMaterial.OwnerId, caKeyMaterial);

            var parser = new X509CertificateParser();
            // NOTE: In C#, ReadCertificate expects bytes, not string:
            var issuerBc = parser.ReadCertificate(Encoding.ASCII.GetBytes(issuer.PemCertificate));

            var issuerRsa = issuerX509.GetRSAPrivateKey() ?? throw new Exception("ISSUER_RSA_NOT_FOUND");
            // Pass RSA directly; GetKeyPair returns both keys in BC form
            var issuerBcKey = DotNetUtilities.GetKeyPair(issuerRsa).Private;

            // 4) Create and sign the new intermediate
            var (keyPair, bcCert) = _certificateGenerator.CreateIntermediate(
                subject, notBefore, notAfter, pathLen, issuerBc, issuerBcKey, keyUsageFlags);

            var pem = ToPem(bcCert);

            // Ensure a newline between leaf PEM and issuer chain PEM (if needed)
            var join = issuer.ChainPem.StartsWith("-----BEGIN CERTIFICATE-----", StringComparison.Ordinal)
                ? "\n"
                : string.Empty;
            var chainPem = pem + join + issuer.ChainPem;

            var model = new Certificate
            {
                SerialHex = bcCert.SerialNumber.ToString(16),
                SubjectDistinguishedName = bcCert.SubjectDN.ToString(),
                IssuerDistinguishedName = bcCert.IssuerDN.ToString(),
                NotBeforeUtc = bcCert.NotBefore.ToUniversalTime(),
                NotAfterUtc = bcCert.NotAfter.ToUniversalTime(),
                IsCertificateAuthority = true,
                PathLenConstraint = pathLen,
                Type = CertificateType.IntermediateCa,
                PemCertificate = pem,
                ChainPem = chainPem,
                ParentCertificateId = issuer.Id,
                ChainRootId = issuer.ChainRootId
            };

            var newId = await _certificateRepository.InsertAsync(model);

            // 5) Save keystore for the new intermediate (private key + chain)
            var leafX509 = BcToX509WithPrivateKey(bcCert, keyPair);

            // Parse the issuer's chain (issuer.ChainPem contains issuer + its parents up to root)
            var issuerChainCollection = ParseChainPem(issuer.ChainPem);

            await _keystoreService.SaveCaPfxAsync(targetCaUserId, newId, leafX509, issuerChainCollection);

            // 6) Ensure CA assignment exists for the target user
            var chainRootId = issuer.ChainRootId > 0 ? issuer.ChainRootId : newId;
            var existingAssignment = await _caAssignmentRepository.IsChainAssignedToUserAsync(targetCaUserId, chainRootId);
            if (!existingAssignment)
            {
                await _caAssignmentRepository.InsertAsync(new CaAssignment
                {
                    CaUserId = targetCaUserId,
                    ChainRootCertificateId = chainRootId,
                    IsActive = true,
                    AssignedByUserId = assignedByUserId
                });
            }

            return (await _certificateRepository.GetByIdAsync(newId))!;
        }

        // ---- Query methods ----
        public async Task<List<Certificate>> GetAllCACertificatesAsync()
        {
            var allCerts = await _certificateRepository.GetAllAsync();
            return allCerts.Where(c => c.IsCertificateAuthority).ToList();
        }

        public async Task<List<Certificate>> GetUserCACertificatesAsync(int caUserId)
        {
            // Dohvati sve chain root ID-jeve dodeljene ovom korisniku
            var assignedChainRoots = await _caAssignmentRepository.GetAssignedChainRootsForUserAsync(caUserId);

            if (assignedChainRoots.Count == 0)
                return new List<Certificate>();

            // Dohvati sve CA sertifikate koji pripadaju tim lancima
            // SIGURNOST: CA korisnik može da vidi SAMO intermediate CA sertifikate čiji je on vlasnik privatnog ključa
            // Ovo sprečava:
            // 1. Pristup root CA sertifikatima (Type != RootCa)
            // 2. Pristup sertifikatima drugih CA korisnika iz istog lanca (filtrira se kroz CaKeyMaterial.OwnerId)
            var allCerts = await _certificateRepository.GetAllAsync();

            // Za svaki kandidat sertifikat, proveri da li korisnik poseduje privatni ključ
            var result = new List<Certificate>();
            foreach (var cert in allCerts.Where(c => c.IsCertificateAuthority
                                                  && assignedChainRoots.Contains(c.ChainRootId)
                                                  && c.Type != CertificateType.RootCa))
            {
                var keyMaterial = await _certificateRepository.GetCaKeyMaterialAsync(cert.Id);
                if (keyMaterial != null && keyMaterial.OwnerId == caUserId)
                {
                    result.Add(cert);
                }
            }

            return result;
        }

        // ---- helpers ----
        private static string ToPem(Org.BouncyCastle.X509.X509Certificate bcCert) =>
            "-----BEGIN CERTIFICATE-----\n" +
            Convert.ToBase64String(bcCert.GetEncoded(), Base64FormattingOptions.InsertLineBreaks) +
            "\n-----END CERTIFICATE-----\n";

        private static X509Certificate2 BcToX509WithPrivateKey(
            Org.BouncyCastle.X509.X509Certificate bcCert,
            Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair)
        {
            var dotNetCert = new X509Certificate2(bcCert.GetEncoded());
            var rsa = DotNetUtilities.ToRSA((Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters)keyPair.Private);
            return dotNetCert.CopyWithPrivateKey(rsa);
        }

        /// <summary>
        /// Parses a multi-cert PEM string (ChainPem) into X509Certificate2Collection.
        /// Extracts all -----BEGIN CERTIFICATE----- blocks.
        /// </summary>
        private static X509Certificate2Collection ParseChainPem(string chainPem)
        {
            var collection = new X509Certificate2Collection();

            if (string.IsNullOrWhiteSpace(chainPem))
                return collection;

            // Match all PEM certificate blocks
            var matches = System.Text.RegularExpressions.Regex.Matches(chainPem,
                @"-----BEGIN CERTIFICATE-----\s*(.*?)\s*-----END CERTIFICATE-----",
                System.Text.RegularExpressions.RegexOptions.Singleline);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                try
                {
                    var pemBlock = "-----BEGIN CERTIFICATE-----\n" +
                                   match.Groups[1].Value +
                                   "\n-----END CERTIFICATE-----";
                    var certBytes = Encoding.ASCII.GetBytes(pemBlock);
                    var cert = new X509Certificate2(certBytes);
                    collection.Add(cert);
                }
                catch
                {
                    // Skip invalid PEM blocks
                }
            }

            return collection;
        }
    }
}
