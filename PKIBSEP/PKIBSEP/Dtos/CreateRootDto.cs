namespace PKIBSEP.Dtos
{
    public class CreateRootDto
    {
        public X500NameDto Subject { get; set; } = default!;
        /// <summary>Validity period in days (default 10 years).</summary>
        public int ValidityDays { get; set; } = 3650;
        /// <summary>
        /// BasicConstraints pathLenConstraint:
        /// null = unlimited; 0 = may only issue EE below; N&gt;0 = allowed CA depth below.
        /// </summary>
        public int? PathLenConstraint { get; set; }
        /// <summary>
        /// Key usage flags for the certificate. If null, defaults to KeyCertSign | CrlSign.
        /// </summary>
        public KeyUsageDto? KeyUsage { get; set; }
    }
}
