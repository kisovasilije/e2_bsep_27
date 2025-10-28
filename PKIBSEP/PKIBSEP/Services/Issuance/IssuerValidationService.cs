using PKIBSEP.Interfaces;
using PKIBSEP.Interfaces.Repository;

namespace PKIBSEP.Services.Issuance
{
    public class IssuerValidationService : IIssuerValidationService
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IChainService _chainService;

        public IssuerValidationService(ICertificateRepository certificateRepository, IChainService chainService)
        {
            _certificateRepository = certificateRepository;
            _chainService = chainService;
        }

        /// <summary>
        /// Validates an issuer before issuing a new certificate.
        /// Ensures:
        /// - issuer is within its validity window (NotBefore/NotAfter),
        /// - issuer is not revoked,
        /// - issuer is a CA (BasicConstraints cA=true),
        /// - if we are issuing another CA, issuer's pathLenConstraint allows it,
        /// - issuer's chain has valid signatures up to a self-signed root.
        /// </summary>
        /// <param name="issuerCertificateId">Database ID of the issuer certificate that will sign the new certificate.</param>
        /// <param name="isIssuingCa">
        /// True if the new certificate to be issued is a CA (root/intermediate); 
        /// False if it's an End-Entity. Used to enforce pathLenConstraint when issuing a CA.
        /// </param>
        /// <param name="requestedPathLength">
        /// Optional pathLenConstraint for the **new** CA being issued. 
        /// </param>
        public async Task ValidateAsync(int issuerCertificateId, bool isIssuingCa, int? requestedPathLength)
        {
            var nowUtc = DateTime.UtcNow;

            var issuer = await _certificateRepository.GetByIdAsync(issuerCertificateId)
                         ?? throw new Exception("ISSUER_NOT_FOUND");

            // 1) Revocation check
            if (issuer.IsRevoked)
                throw new Exception("ISSUER_REVOKED");

            // 2) Time window check
            if (nowUtc < issuer.NotBeforeUtc || nowUtc > issuer.NotAfterUtc)
                throw new Exception("ISSUER_TIME_INVALID");

            // 3) Must be a CA
            if (!issuer.IsCertificateAuthority)
                throw new Exception("ISSUER_NOT_CA");

            // 4) pathLenConstraint enforcement for issuing a new CA
            //    If issuer's pathLenConstraint == 0 → it cannot issue another CA below it.
            if (isIssuingCa && issuer.PathLenConstraint == 0)
                throw new Exception("PATHLEN_BLOCKS_CA");

            // 5) Cryptographic chain verification (issuer → ... → root)
            var chainIsValid = await _chainService.VerifyIssuerChainAsync(issuerCertificateId);
            if (!chainIsValid)
                throw new Exception("CHAIN_SIGNATURE_INVALID");
        }

    }
}
