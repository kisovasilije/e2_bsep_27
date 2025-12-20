namespace PKIBSEP.Dtos.Certificates;

public record CsrResponseDto(
    string clientCertPem,
    string caCertPem,
    string serialNumberHex);
