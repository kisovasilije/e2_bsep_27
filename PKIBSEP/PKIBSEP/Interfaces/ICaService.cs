using FluentResults;
using PKIBSEP.Dtos.Certificates;

namespace PKIBSEP.Interfaces;

public interface ICaService
{
    Task<(string clientCertPem, string caCertPem, string serialNumberHex)> SignCsrAsync (CertificateSigningRequestDto csr, int userId);

    Task<Result<IEnumerable<CaDto>>> GetCAsAsync ();

    Task<Result<IEnumerable<CertificatePreviewDto>>> GetCertificatesByUserIdAsync (int userId);

    Task<Result<CertificatePreviewDto>> RevokeCertificateAsync (RevocationRequestDto request, int userId);

    Task<byte[]> GetOcspResponseAsync(byte[] ocspRequestBytes);
}
