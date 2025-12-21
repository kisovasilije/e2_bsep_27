using PKIBSEP.Common.Enum;

namespace PKIBSEP.Models.Certificate;

public class Certificate2
{
    public int Id { get; init; }

    public string SerialNumberHex { get; private set; }

    public int SubjectId { get; init; }

    public int IssuerId { get; init; }

    public string Pem { get; private set; }

    public string SubjectDn { get; init; }

    public DateTime NotBefore { get; private set; }

    public DateTime NotAfter { get; private set; }

    public string PrivateKeyRef { get; init; }

    public string CertRef { get; init; }

    public CertificateType Type { get; private set; }

    public User? Subject { get; private set; } = null;

    public Certificate2? Issuer { get; private set; } = null;

    public Certificate2(
        int id,
        string serialNumberHex,
        int subjectId,
        int issuerId,
        string pem,
        string subjectDn,
        DateTime notBefore,
        DateTime notAfter,
        string privateKeyRef,
        string certRef,
        CertificateType type)
    {
        Id = id;
        SerialNumberHex = serialNumberHex;
        SubjectId = subjectId;
        IssuerId = issuerId;
        Pem = pem;
        SubjectDn = subjectDn;
        NotBefore = notBefore;
        NotAfter = notAfter;
        PrivateKeyRef = privateKeyRef;
        CertRef = certRef;
        Type = type;
    }

    public Certificate2 (
        string serialNumberHex,
        int subjectId,
        int issuerId,
        string pem,
        string subjectDn,
        DateTime notBefore,
        DateTime notAfter,
        string privateKeyRef,
        string certRef,
        CertificateType type)
    {
        SerialNumberHex = serialNumberHex;
        SubjectId = subjectId;
        IssuerId = issuerId;
        Pem = pem;
        SubjectDn = subjectDn;
        NotBefore = notBefore;
        NotAfter = notAfter;
        PrivateKeyRef = privateKeyRef;
        CertRef = certRef;
        Type = type;
    }
}
