using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using PKIBSEP.Common;
using PKIBSEP.Interfaces;
using System.Security.Cryptography;

namespace PKIBSEP.Services;

public class CaService : ICaService
{
    private static int rsaMinKeySizeBits = 2048;

    private readonly AsymmetricKeyParameter caPrivateKey;

    private readonly X509Certificate caCert;

    private readonly string caCertPem;

    private readonly int defaultValidityDays;

    public CaService(IOptions<CaOptions> opts)
    {
        var options = opts.Value;

        if (string.IsNullOrEmpty(options.PrivateKeyPath))
        {
            throw new ArgumentException("CA private key path is not configured.");
        }

        if (string.IsNullOrEmpty(options.CertificatePath))
        {
            throw new ArgumentException("CA certificate path is not configured.");
        }

        defaultValidityDays = options.DefaultValidityDays <= 0 ? 365 : options.DefaultValidityDays;
        caPrivateKey = ReadPrivateKeyFromPemFile(options.PrivateKeyPath);
        caCert = ReadCertificateFromPemFile(options.CertificatePath);
        caCertPem = File.ReadAllText(options.CertificatePath);
    }

    public (string clientCertPem, string caCertPem, string serialNumberHex) SignCsr(string csrPem)
    {
        if (string.IsNullOrWhiteSpace(csrPem))
        {
            throw new ArgumentException("CSR PEM content is empty.");
        }

        var csr = ParseCsr(csrPem);
        if (!csr.Verify())
        {
            throw new InvalidOperationException("CSR signature verification failed.");
        }

        var csrInfo = csr.GetCertificationRequestInfo();
        var subject = csrInfo.Subject;
        AsymmetricKeyParameter publicKey = csr.GetPublicKey();

        EnforceRsaKeyMinSize(publicKey, rsaMinKeySizeBits);

        string serialNumberHex;
        var serial = GenerateSerialNumber(out serialNumberHex);

        var now = DateTimeOffset.UtcNow;
        var notBefore = now.AddMinutes(-5);
        var notAfter = now.AddDays(defaultValidityDays);

        var gen = new X509V3CertificateGenerator();
        gen.SetSerialNumber(serial);
        gen.SetIssuerDN(caCert.SubjectDN);
        gen.SetNotBefore(notBefore.UtcDateTime);
        gen.SetNotAfter(notAfter.UtcDateTime);
        gen.SetSubjectDN(subject);
        gen.SetPublicKey(publicKey);

        gen.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));
        gen.AddExtension(
            X509Extensions.KeyUsage,
            true,
            new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));

        gen.AddExtension(
            X509Extensions.ExtendedKeyUsage,
            false,
            new ExtendedKeyUsage(new[]
            {
                KeyPurposeID.IdKPServerAuth,
                KeyPurposeID.IdKPClientAuth,
            }));

        gen.AddExtension(
            X509Extensions.SubjectKeyIdentifier,
            false,
            new SubjectKeyIdentifierStructure(publicKey));

        gen.AddExtension(
            X509Extensions.AuthorityKeyIdentifier,
            false,
            new AuthorityKeyIdentifierStructure(caCert));

        var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", caPrivateKey);
        X509Certificate clientCert = gen.Generate(signatureFactory);

        string clientCertPem = ConvertToPem(clientCert);
        return (clientCertPem, caCertPem, serialNumberHex);
    }

    private static Pkcs10CertificationRequest ParseCsr(string csrPem)
    {
        using var sr = new StringReader(csrPem);
        var pr = new PemReader(sr);
        var obj = pr.ReadObject();

        if (obj is Pkcs10CertificationRequest csr)
        {
            return csr;
        }

        throw new InvalidOperationException("The provided PEM does not contain a valid CSR.");
    }

    private static void EnforceRsaKeyMinSize (AsymmetricKeyParameter publicKey, int minBits)
    {
        if (publicKey is not RsaKeyParameters rsaKey || rsaKey.IsPrivate)
        {
            throw new InvalidOperationException("The public key is not a valid RSA public key.");
        }

        int bitLength = rsaKey.Modulus.BitLength;
        if (bitLength < minBits)
        {
            throw new InvalidOperationException($"The RSA key size is too small. Minimum required is {minBits} bits.");
        }
    }

    private static BigInteger GenerateSerialNumber(out string serialNumberHex)
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);

        bytes[0] &= 0x7F;

        serialNumberHex = Convert.ToHexString(bytes).ToLowerInvariant();
        return new BigInteger(1, bytes.ToArray());
    }

    private static AsymmetricKeyParameter ReadPrivateKeyFromPemFile (string filePath)
    {
        var pem = File.ReadAllText(filePath);
        using var sr = new StringReader(pem);
        var pr = new PemReader(sr);
        var obj = pr.ReadObject();

        return obj switch
        {
            AsymmetricCipherKeyPair keyPair => keyPair.Private,
            AsymmetricKeyParameter keyParam when keyParam.IsPrivate => keyParam,
            _ => throw new InvalidOperationException("The PEM file does not contain a valid private key.")
        };
    }

    private static X509Certificate ReadCertificateFromPemFile (string filePath)
    {
        var pem = File.ReadAllText(filePath);
        using var sr = new StringReader(pem);
        var pr = new PemReader(sr);
        var obj = pr.ReadObject();

        if (obj is X509Certificate cert)
        {
            return cert;
        }
        throw new InvalidOperationException("The PEM file does not contain a valid X509 certificate.");
    }

    private static string ConvertToPem(X509Certificate certificate)
    {
        using var sw = new StringWriter();
        var pemWriter = new PemWriter(sw);
        pemWriter.WriteObject(certificate);
        pemWriter.Writer.Flush();
        return sw.ToString();
    }
}
