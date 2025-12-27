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

    public string? CsrHashHex { get; private set; }

    public DateTime? RevokedAt { get; private set; } = null;

    public RevocationReason? RevocationReason { get; private set; } = null;

    public string? RevocationComment { get; private set; } = string.Empty;

    public int? RevokedBy { get; private set; } = null;

    public bool IsRevoked { get; private set; } = false;

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
        CertificateType type,
        string csrHashHex,
        DateTime? revokedAt = null,
        RevocationReason? revocationReason = null,
        string? revocationComment = null,
        int? revokedBy = null,
        bool isRevoked = false)
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
        CsrHashHex = csrHashHex;
        RevokedAt = revokedAt;
        RevocationReason = revocationReason;
        RevocationComment = revocationComment;
        RevokedBy = revokedBy;
        IsRevoked = isRevoked;
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

    public bool IsRevocationRequestValid(int userId)
    {
        return !IsRevoked && SubjectId == userId;
    }

    public void Revoke(RevocationReason reason, string? comment)
    {
        RevokedAt = DateTime.UtcNow;
        RevocationReason = reason;
        RevocationComment = comment;
        RevokedBy = SubjectId;
        IsRevoked = true;
    }
}
