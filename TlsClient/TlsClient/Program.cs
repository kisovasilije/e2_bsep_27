using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

namespace TlsClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var inServerPath = @"D:\faks\bezbednost\projekat\certs\certificate_9.pem";
            var issuerPath = @"D:\faks\bezbednost\projekat\certs\int1\int1.crt.pem";

            var inServerText = await File.ReadAllTextAsync(inServerPath);
            var issuerText = await File.ReadAllTextAsync(issuerPath);

            using var serverCert = X509Certificate2.CreateFromPem(inServerText);
            using var issuerCert = X509Certificate2.CreateFromPem(issuerText);

            Org.BouncyCastle.X509.X509Certificate bcServerCert = DotNetUtilities.FromX509Certificate(serverCert);
            Org.BouncyCastle.X509.X509Certificate bcIssuerCert = DotNetUtilities.FromX509Certificate(issuerCert);

            string? ocspUrl = null;

            var aiaExt = bcServerCert.GetExtensionValue(X509Extensions.AuthorityInfoAccess);
            if (aiaExt is null)
            {
                Console.WriteLine("Missing aia extension from server certificate.");
                return;
            }

            var aia = AuthorityInformationAccess.GetInstance(Asn1Object.FromByteArray(aiaExt.GetOctets()));
            var ad = aia.GetAccessDescriptions()[0];
            if (ad is null)
            {
                Console.WriteLine("Missing access description from aia.");
                return;
            }

            if (ad.AccessMethod.Equals(AccessDescription.IdADOcsp))
            {
                var name = ad.AccessLocation;
                if (name.TagNo == GeneralName.UniformResourceIdentifier)
                {
                    ocspUrl = DerIA5String.GetInstance(name.Name).GetString();
                }
            }

            if (string.IsNullOrEmpty(ocspUrl))
            {
                Console.WriteLine("Missing ocsp responder url from accesss description.");
                return;
            }

            CertificateID certId = new CertificateID(
                CertificateID.HashSha1,
                bcIssuerCert,
                bcServerCert.SerialNumber
            );

            OcspReqGenerator gen = new OcspReqGenerator();

            gen.AddRequest(certId);

            DerOctetString nonce = new(Guid.NewGuid().ToByteArray());

            X509Extensions extensions = new([Org.BouncyCastle.Asn1.Ocsp.OcspObjectIdentifiers.PkixOcspNonce], [new Org.BouncyCastle.Asn1.X509.X509Extension(false, nonce)]);

            gen.SetRequestExtensions(extensions);

            OcspReq ocspReq = gen.Generate();
            byte[] ocspRequestBytes = ocspReq.GetEncoded();

            using var http = new HttpClient();
            using var content = new ByteArrayContent(ocspRequestBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/ocsp-request");

            using var response = await http.PostAsync(ocspUrl, content);
            response.EnsureSuccessStatusCode();
            byte[] ocspResponseBytes = await response.Content.ReadAsByteArrayAsync();
            var ocspResp = new OcspResp(ocspResponseBytes);
            Console.WriteLine($"RESPONSE STATUS: {ocspResp.Status}");
            if (ocspResp.Status != OcspRespStatus.Successful)
            {
                Console.WriteLine($"OCSP response failed: {ocspResp.Status}");
                return;
            }

            var basicResp = (BasicOcspResp) ocspResp.GetResponseObject();
            foreach (var singleResp in basicResp.Responses)
            {
                var certStatus = singleResp.GetCertStatus();

                if (certStatus == CertificateStatus.Good)
                {
                    Console.WriteLine("CERT STATUS: GOOD");
                }
                else if (certStatus is RevokedStatus revoked)
                {
                    Console.WriteLine($"CERT STATUS: REVOKED");
                    Console.WriteLine($"Revocation time: {revoked.RevocationTime}");
                }
                else if (certStatus is UnknownStatus)
                {
                    Console.WriteLine("CERT STATUS: UNKNOWN");
                }
            }
        }
    }
}
