using PKIBSEP.Common.Enum;

namespace PKIBSEP.Dtos.Certificates;

public class RevocationRequestDto
{
    public int CertificateId { get; set; }

    public RevocationReason Reason { get; set; }

    public string? Comment { get; set; }
}
