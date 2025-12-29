using FluentResults;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using PKIBSEP.Common;
using PKIBSEP.Dtos.Certificates;
using PKIBSEP.Interfaces;
using PKIBSEP.Models.Certificate;
using PKIBSEP.Utils;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PKIBSEP.Services;

public class CaService : ICaService
{
    private static int rsaMinKeySizeBits = 2048;

    private readonly string caPrivateKeyBasePath;

    private readonly string caCertificateBasePath;

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

        this.caRepository = caRepository;
    }

    public async Task<(string clientCertPem, string caCertPem, string serialNumberHex)> SignCsrAsync (CertificateSigningRequestDto req, int userId)
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

        var csrHashHex = ComputeCsrHashHex(csr);
        if (await caRepository.ExistsAsync(csrHashHex, ca.Id))
        {
            throw new InvalidOperationException("A certificate has already been issued for this CSR under the selected CA.");
        }

        EnforceDateValidity(ca, req);

        var caPrivateKey = ReadPrivateKeyFromPemFile(GetPath(caPrivateKeyBasePath, ca.PrivateKeyRef));
        var caCert = ReadCertificateFromPemFile(ca.Pem);

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
            new ExtendedKeyUsage([KeyPurposeID.IdKPServerAuth, KeyPurposeID.IdKPClientAuth]));

        gen.AddExtension(
            X509Extensions.SubjectKeyIdentifier,
            false,
            new SubjectKeyIdentifierStructure(publicKey));

        gen.AddExtension(
            X509Extensions.AuthorityKeyIdentifier,
            false,
            new AuthorityKeyIdentifierStructure(caCert));

        var aia = new AuthorityInformationAccess(new AccessDescription(
            AccessDescription.IdADOcsp,
            new GeneralName(GeneralName.UniformResourceIdentifier, "http://localhost:5096/api/ocsp")));

        gen.AddExtension(X509Extensions.AuthorityInfoAccess, false, aia);

        var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", caPrivateKey);
        X509Certificate clientCert = gen.Generate(signatureFactory);
        string clientCertPem = ConvertToPem(clientCert);

        await caRepository.CreateAsync(new Certificate2(
            0,
            serialNumberHex,
            userId,
            ca.Id,
            clientCertPem,
            subject.ToString(),
            req.NotBefore,
            req.NotAfter,
            string.Empty,
            string.Empty,
            Common.Enum.CertificateType.EndEntity,
            csrHashHex));

        return (clientCertPem, ca.Pem, serialNumberHex);
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

        serialNumberHex = Convert.ToHexString(bytes).ToUpperInvariant();
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

    private static X509Certificate ReadCertificateFromPemFile (string pem)
    {
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

    private static string GetPath(string basePath, string fileName)
    {
        return Path.Combine(basePath, fileName);
    }

    private static string ComputeCsrHashHex(Pkcs10CertificationRequest csr)
    {
        byte[] csrDer = csr.GetEncoded();
        using var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(csrDer);
        return Convert.ToHexString(hash);
    }

    public async Task<Result<IEnumerable<CertificatePreviewDto>>> GetCertificatesByUserIdAsync(int userId)
    {
        var certs = await caRepository.GetAllWithIssuerByUserId(userId);
        return Result.Ok(certs.ToCertificatePreviewDtos());
    }

    public async Task<Result<CertificatePreviewDto>> RevokeCertificateAsync(RevocationRequestDto request, int userId)
    {
        if (!await caRepository.ExistsAsync(request.CertificateId))
        {
            return Result.Fail("Certificate requesting revocation does not exist.");
        }

        if (!Enum.IsDefined(typeof(Common.Enum.RevocationReason), request.Reason))
        {
            return Result.Fail("Invalid revocation reason specified.");
        }

        var certificate = (await caRepository.GetByIdAsync(request.CertificateId))!;
        if (!certificate.IsRevocationRequestValid(userId))
        {
            return Result.Fail("Certificate validation error occurred.");
        }

        certificate.Revoke(request.Reason, request.Comment);

        await caRepository.SaveChangesAsync();
        return Result.Ok(certificate.ToCertificatePreviewDto());
    }

    public async Task<byte[]> GetOcspResponseAsync(byte[] ocspRequestBytes)
    {
        OcspReq ocspReq = new OcspReq(ocspRequestBytes);
        Req req = ocspReq.GetRequestList().Single();
        CertificateID reqCertId = req.GetCertID();

        var serialNumberHex = reqCertId.SerialNumber.ToString(16).ToUpperInvariant();

        var intermediates = await caRepository.GetIntermediateCaCerttificatesAsync();
        var issuer = intermediates.FirstOrDefault(c =>
        {
            var cert = ReadCertificateFromPemFile(c.Pem);
            var computedId = new CertificateID(
                reqCertId.HashAlgOid,
                cert,
                reqCertId.SerialNumber);

            bool issuerMatches =
                computedId.GetIssuerNameHash().SequenceEqual(reqCertId.GetIssuerNameHash()) &&
                computedId.GetIssuerKeyHash().SequenceEqual(reqCertId.GetIssuerKeyHash());

            return issuerMatches;
        });

        CertificateStatus certStatus;

        if (issuer is null)
        {
            certStatus = new UnknownStatus();
            return BuildOcspResponse(ocspReq, reqCertId, certStatus);
        }

        var server = await caRepository.GetByIssuerIdAndSerialNumber(issuer.Id, serialNumberHex);
        if (server is null)
        {
            certStatus = new UnknownStatus();
            return BuildOcspResponse(ocspReq, reqCertId, certStatus);
        }

        var now = DateTime.UtcNow;
        if (now < server.NotBefore || now > server.NotAfter)
        {
            certStatus = new UnknownStatus();
            return BuildOcspResponse(ocspReq, reqCertId, certStatus);
        }

        if (server.IsRevoked)
        {
            certStatus = new RevokedStatus(
                server.RevokedAt!.Value,
                (int) (server.RevocationReason ?? Common.Enum.RevocationReason.Unspecified));
            return BuildOcspResponse(ocspReq, reqCertId, certStatus);
        }

        certStatus = CertificateStatus.Good;
        return BuildOcspResponse(ocspReq, reqCertId, certStatus);
    }

    private static byte[] BuildOcspResponse(OcspReq ocspReq, CertificateID reqCertId, CertificateStatus certStatus)
    {
        var responderPem = File.ReadAllText(Common.Environment.OcspCertificatePath);
        var responderCert = ReadCertificateFromPemFile(responderPem);
        var responderPrivateKey = ReadPrivateKeyFromPemFile(Common.Environment.OcspPrivateKeyPath);
        var responderPublicKey = responderCert.GetPublicKey();

        var gen = new BasicOcspRespGenerator(responderPublicKey);

        gen.AddResponse(
            reqCertId,
            certStatus,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(12),
            null);

        var nonceExt = ocspReq.RequestExtensions?
            .GetExtension(OcspObjectIdentifiers.PkixOcspNonce);

        if (nonceExt is not null)
        {
            gen.SetResponseExtensions(new X509Extensions([OcspObjectIdentifiers.PkixOcspNonce], [nonceExt]));
        }

        BasicOcspResp basicResp = gen.Generate(
            "SHA256withRSA",
            responderPrivateKey,
            [responderCert],
            DateTime.UtcNow);

        OcspResp ocspResp = new OCSPRespGenerator().Generate(OcspRespStatus.Successful, basicResp);
        return ocspResp.GetEncoded();
    }
}
