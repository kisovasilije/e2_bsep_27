namespace PKIBSEP.Interfaces;

public interface ICaService
{
    (string clientCertPem, string caCertPem, string serialNumberHex) SignCsr (string csrPem);
}
