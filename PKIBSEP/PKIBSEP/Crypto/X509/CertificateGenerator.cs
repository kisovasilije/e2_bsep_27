using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509.Extension;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Math;
using PKIBSEP.Interfaces; // <-- BouncyCastle BigInteger

namespace PKIBSEP.Crypto.X509
{
    /// <summary>
    /// Generates X.509 v3 certificates using BouncyCastle:
    /// - self-signed Root CA
    /// - Intermediate CA signed by an issuer CA
    /// Adds CA extensions: BasicConstraints(cA=true, pathLen), KeyUsage(keyCertSign|cRLSign), SKI/AKI.
    /// </summary>
    public sealed class CertificateGenerator: ICertificateGenerator
    {
        /// <summary>
        /// Creates a self-signed Root CA certificate.
        /// </summary>
        public (AsymmetricCipherKeyPair KeyPair, X509Certificate Certificate) CreateSelfSignedRoot(
            X509Name subject,
            DateTime notBeforeUtc,
            DateTime notAfterUtc,
            int? pathLengthConstraint,
            int keyUsageFlags)
        {
            var keyPair = GenerateRsaKeyPair(3072);

            var gen = new X509V3CertificateGenerator();
            gen.SetSerialNumber(CreatePositiveRandomSerial());
            gen.SetIssuerDN(subject);
            gen.SetSubjectDN(subject);
            gen.SetNotBefore(notBeforeUtc);
            gen.SetNotAfter(notAfterUtc);
            gen.SetPublicKey(keyPair.Public);

            // Critical CA extensions
            gen.AddExtension(X509Extensions.BasicConstraints, true,
                new BasicConstraints(pathLengthConstraint ?? int.MaxValue));
            gen.AddExtension(X509Extensions.KeyUsage, true,
                new KeyUsage(keyUsageFlags));

            // SKI/AKI (use helper utilities in C#)
            var ski = X509ExtensionUtilities.CreateSubjectKeyIdentifier(keyPair.Public);
            gen.AddExtension(X509Extensions.SubjectKeyIdentifier, false, ski);

            // For self-signed root, AKI is derived from the same public key
            var aki = X509ExtensionUtilities.CreateAuthorityKeyIdentifier(keyPair.Public);
            gen.AddExtension(X509Extensions.AuthorityKeyIdentifier, false, aki);

            var certificate = gen.Generate(new Asn1SignatureFactory("SHA256WITHRSA", keyPair.Private));
            return (keyPair, certificate);
        }

        /// <summary>
        /// Creates an Intermediate CA certificate signed by an issuer CA.
        /// </summary>
        public (AsymmetricCipherKeyPair KeyPair, X509Certificate Certificate) CreateIntermediate(
            X509Name subject,
            DateTime notBeforeUtc,
            DateTime notAfterUtc,
            int? pathLengthConstraint,
            X509Certificate issuerCertificate,
            AsymmetricKeyParameter issuerPrivateKey,
            int keyUsageFlags)
        {
            var keyPair = GenerateRsaKeyPair(3072);

            var gen = new X509V3CertificateGenerator();
            gen.SetSerialNumber(CreatePositiveRandomSerial());
            gen.SetIssuerDN(issuerCertificate.SubjectDN);
            gen.SetSubjectDN(subject);
            gen.SetNotBefore(notBeforeUtc);
            gen.SetNotAfter(notAfterUtc);
            gen.SetPublicKey(keyPair.Public);

            // Critical CA extensions
            gen.AddExtension(X509Extensions.BasicConstraints, true,
                new BasicConstraints(pathLengthConstraint ?? int.MaxValue));
            gen.AddExtension(X509Extensions.KeyUsage, true,
                new KeyUsage(keyUsageFlags));

            // Identification: SKI for subject; AKI must reference issuer
            var ski = X509ExtensionUtilities.CreateSubjectKeyIdentifier(keyPair.Public);
            gen.AddExtension(X509Extensions.SubjectKeyIdentifier, false, ski);

            var aki = X509ExtensionUtilities.CreateAuthorityKeyIdentifier(issuerCertificate);
            gen.AddExtension(X509Extensions.AuthorityKeyIdentifier, false, aki);

            var certificate = gen.Generate(new Asn1SignatureFactory("SHA256WITHRSA", issuerPrivateKey));
            return (keyPair, certificate);
        }

        // ---------- internals ----------

        private static AsymmetricCipherKeyPair GenerateRsaKeyPair(int keySizeBits)
        {
            var generator = new RsaKeyPairGenerator();
            generator.Init(new KeyGenerationParameters(new SecureRandom(), keySizeBits));
            return generator.GenerateKeyPair();
        }

        /// <summary>
        /// Creates a positive random BigInteger serial number (128-bit).
        /// </summary>
        private static BigInteger CreatePositiveRandomSerial(int numBytes = 16)
        {
            var rnd = new SecureRandom();
            var bytes = new byte[numBytes];
            rnd.NextBytes(bytes);

            // Ensure positive (BigInteger(1, bytes) means sign=positive)
            return new BigInteger(1, bytes);
        }
    }
}
