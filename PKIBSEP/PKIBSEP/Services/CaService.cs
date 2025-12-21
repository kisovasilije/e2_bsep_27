using FluentResults;
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
using PKIBSEP.Dtos.Certificates;
using PKIBSEP.Interfaces;
using PKIBSEP.Models.Certificate;
using System.Security.Cryptography;

namespace PKIBSEP.Services;

public class CaService : ICaService
{
    private static int rsaMinKeySizeBits = 2048;

    //private readonly AsymmetricKeyParameter caPrivateKey;

    //private readonly X509Certificate caCert;

    private readonly string caPrivateKeyBasePath;

    private readonly string caCertificateBasePath;

    //private readonly string caCertPem;

    private readonly int defaultValidityDays;

    private readonly ICaRepository caRepository;

    public CaService(IOptions<CaOptions> opts, ICaRepository caRepository)
    {
        var options = opts.Value;

        if (string.IsNullOrEmpty(options.PrivateKeyBasePath))
        {
            throw new ArgumentException("CA private key path is not configured.");
        }

        if (string.IsNullOrEmpty(options.CertificateBasePath))
        {
            throw new ArgumentException("CA certificate path is not configured.");
        }

        defaultValidityDays = 365;
        caPrivateKeyBasePath = options.PrivateKeyBasePath;
        caCertificateBasePath = options.CertificateBasePath;
        //caPrivateKey = ReadPrivateKeyFromPemFile(options.PrivateKeyPath);
        //caCert = ReadCertificateFromPemFile(options.CertificatePath);
        //caCertPem = File.ReadAllText(options.CertificatePath);

        this.caRepository = caRepository;
    }

    public async Task<(string clientCertPem, string caCertPem, string serialNumberHex)> SignCsrAsync (CertificateSigningRequestDto req)
    {
        if (string.IsNullOrWhiteSpace(req.CsrPem))
        {
            throw new ArgumentException("CSR PEM content is empty.");
        }

        var csr = ParseCsr(req.CsrPem);
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

        var ca = await caRepository.GetCaByIdAsync(req.CaId);
        if (ca == null)
        {
            throw new InvalidOperationException($"CA with ID {req.CaId} not found.");
        }

        EnforceDateValidity(ca, req);

        var (caPrivateKey, caCert, caCertPem) = GetCaKeyAndCert(
            GetPath(caPrivateKeyBasePath, ca.PrivateKeyRef),
            GetPath(caCertificateBasePath, ca.CertRef));

        var gen = new X509V3CertificateGenerator();
        gen.SetSerialNumber(serial);
        gen.SetIssuerDN(caCert.SubjectDN);
        gen.SetNotBefore(req.NotBefore);
        gen.SetNotAfter(req.NotAfter);
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

    public async Task<Result<IEnumerable<CaDto>>> GetCAsAsync()
    {
        var certs = await caRepository.GetAllCAsAsync();
        return Result.Ok(certs.Select(c => new CaDto(
            Id: c.Id,
            SubjectDn: c.SubjectDn,
            NotBefore: c.NotBefore,
            NotAfter: c.NotAfter)));
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

    private static (AsymmetricKeyParameter caPrivateKeyPem, X509Certificate caCert, string caCertPem) GetCaKeyAndCert(string keyPath, string certPath)
    {
        var caPrivateKeyPem = ReadPrivateKeyFromPemFile(keyPath);
        var caCert = ReadCertificateFromPemFile(certPath);
        var caCertPem = File.ReadAllText(certPath);
        return (caPrivateKeyPem, caCert, caCertPem);
    }

    private static void EnforceDateValidity(Certificate2 ca, CertificateSigningRequestDto req)
    {
        if (req.NotBefore < ca.NotBefore.ToUniversalTime())
        {
            req = req with { NotBefore = ca.NotBefore };
        }
        if (req.NotAfter > ca.NotAfter.ToUniversalTime())
        {
            req = req with { NotAfter = ca.NotAfter };
        }
        if (req.NotAfter <= req.NotBefore)
        {
            throw new InvalidOperationException("The requested NotAfter date must be later than NotBefore date.");
        }
    }

    private string GetPath(string basePath, string fileName)
    {
        return Path.Combine(basePath, fileName);
    }
}
