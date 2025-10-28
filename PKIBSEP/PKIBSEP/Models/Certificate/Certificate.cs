using PKIBSEP.Common.Enum;

namespace PKIBSEP.Models.Certificate;

/// <summary>
/// Central record for an X.509 certificate and its position in the chain.
/// Covers issuance metadata, chain navigation, and revocation status.
/// </summary>
public class Certificate
{
    public int Id { get; set; }

    /// <summary>Certificate serial number (hex string, e.g., "01AF...").</summary>
    public string SerialHex { get; set; } = default!;

    /// <summary>Subject DN (canonical X.500 DN string).</summary>
    public string SubjectDistinguishedName { get; set; } = default!;

    /// <summary>Issuer DN (canonical X.500 DN string).</summary>
    public string IssuerDistinguishedName { get; set; } = default!;

    /// <summary>Validity start (UTC).</summary>
    public DateTime NotBeforeUtc { get; set; }

    /// <summary>Validity end (UTC).</summary>
    public DateTime NotAfterUtc { get; set; }

    /// <summary>True if BasicConstraints cA=true.</summary>
    public bool IsCertificateAuthority { get; set; }

    /// <summary>
    /// BasicConstraints.pathLenConstraint:
    /// null = unlimited; 0 = can issue only End-Entity below; N>0 = allowed CA depth below this CA.
    /// </summary>
    public int? PathLenConstraint { get; set; }

    /// <summary>Certificate type (RootCa/IntermediateCa/EndEntity).</summary>
    public CertificateType Type { get; set; }

    /// <summary>(Optional) Owning organization for authorization/policy.</summary>
    public string? Organization { get; set; }

    /// <summary>PEM for this certificate only.</summary>
    public string PemCertificate { get; set; } = default!;

    /// <summary>Concatenated PEM of this certificate + entire chain to root.</summary>
    public string ChainPem { get; set; } = default!;

    /// <summary>Parent certificate id (issuer). Root has null.</summary>
    public int? ParentCertificateId { get; set; }

    /// <summary>Root id of the chain (topmost root Certificate.Id).</summary>
    public int ChainRootId { get; set; }

    /// <summary>(Optional) Owner user id (useful for EE later).</summary>
    public int? OwnerUserId { get; set; }

    /// <summary>Revocation status.</summary>
    public bool IsRevoked { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public RevocationReason? RevocationReason { get; set; }
}
