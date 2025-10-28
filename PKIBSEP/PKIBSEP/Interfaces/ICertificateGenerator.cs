using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

namespace PKIBSEP.Interfaces
{
    public interface ICertificateGenerator
    {
        (AsymmetricCipherKeyPair KeyPair, X509Certificate Certificate)
            CreateSelfSignedRoot(X509Name subject, DateTime notBeforeUtc, DateTime notAfterUtc, int? pathLengthConstraint, int keyUsageFlags);

        (AsymmetricCipherKeyPair KeyPair, X509Certificate Certificate)
            CreateIntermediate(X509Name subject, DateTime notBeforeUtc, DateTime notAfterUtc, int? pathLengthConstraint,
                               X509Certificate issuerCertificate, Org.BouncyCastle.Crypto.AsymmetricKeyParameter issuerPrivateKey, int keyUsageFlags);
    }
}
