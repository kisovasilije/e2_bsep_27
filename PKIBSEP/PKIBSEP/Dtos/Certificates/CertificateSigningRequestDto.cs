namespace PKIBSEP.Dtos.Certificates;

public record CertificateSigningRequestDto(
    int CaId,
    DateTime NotBefore,
    DateTime NotAfter,
    string CsrPem);
