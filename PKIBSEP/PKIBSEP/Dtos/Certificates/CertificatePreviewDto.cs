namespace PKIBSEP.Dtos.Certificates;

public class CertificatePreviewDto
{
    public int Id { get; set; }

    public string IssuedTo { get; set; } = string.Empty;

    public string IssuedBy { get; set; } = string.Empty;

    public DateTime NotBefore { get; set; }

    public DateTime NotAfter { get; set; }

    public string Pem { get; set; } = string.Empty;
}
