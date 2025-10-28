namespace PKIBSEP.Interfaces
{
    public interface IChainService
    {
        /// <summary>
        /// Cryptographically verifies the issuer’s chain (issuer → ... → root)
        /// and validates the root as self-signed.
        /// </summary>
        Task<bool> VerifyIssuerChainAsync(int issuerCertificateId);
    }
}
