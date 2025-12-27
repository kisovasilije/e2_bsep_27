using PKIBSEP.Dtos.Certificates;
using PKIBSEP.Models.Certificate;

namespace PKIBSEP.Utils;

public static class Mapper
{
    public static CertificatePreviewDto ToCertificatePreviewDto(this Certificate2 cert)
    {
        ArgumentNullException.ThrowIfNull(cert);

        return new CertificatePreviewDto
        {
            Id = cert.Id,
            IssuedTo = cert.SubjectDn,
            IssuedBy = cert.Issuer != null ? cert.Issuer.SubjectDn : "Self-signed",
            NotBefore = cert.NotBefore,
            NotAfter = cert.NotAfter,
            IsRevoked = cert.IsRevoked,
            Pem = cert.Pem
        };
    }

    public static IEnumerable<CertificatePreviewDto> ToCertificatePreviewDtos(this IEnumerable<Certificate2> certs)
    {
        ArgumentNullException.ThrowIfNull(certs);

        return certs.Select(cert => cert.ToCertificatePreviewDto());
    }

}
