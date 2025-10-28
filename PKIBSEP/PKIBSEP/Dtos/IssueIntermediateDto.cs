namespace PKIBSEP.Dtos
{
    public class IssueIntermediateDto
    {
        /// <summary>
        /// Target CA user ID. Admin MUST specify this to assign the certificate to a CA user.
        /// CA users CANNOT specify this (will be set to their own ID automatically).
        /// </summary>
        public int? TargetCaUserId { get; set; }

        /// <summary>DB id of the issuer CA certificate.</summary>
        public int IssuerId { get; set; }
        public X500NameDto Subject { get; set; } = default!;
        /// <summary>Requested validity in days (will be clamped to issuer.NotAfter).</summary>
        public int ValidityDays { get; set; } = 1825;
        /// <summary>BasicConstraints pathLenConstraint for the new Intermediate.</summary>
        public int? PathLenConstraint { get; set; }
        /// <summary>
        /// Key usage flags for the certificate. If null, defaults to KeyCertSign | CrlSign.
        /// </summary>
        public KeyUsageDto? KeyUsage { get; set; }
    }
}
