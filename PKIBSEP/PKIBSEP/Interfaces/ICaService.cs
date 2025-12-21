using FluentResults;
using PKIBSEP.Dtos.Certificates;

namespace PKIBSEP.Interfaces;

public interface ICaService
{
    Task<(string clientCertPem, string caCertPem, string serialNumberHex)> SignCsrAsync (CertificateSigningRequestDto csr);

    Task<Result<IEnumerable<CaDto>>> GetCAsAsync ();
}
