namespace PKIBSEP.Dtos
{
    public class CertificateDto
    {
        public int Id { get; set; }
        public string SerialHex { get; set; } = default!;
        public string SubjectDN { get; set; } = default!;
        public string IssuerDN { get; set; } = default!;
        public DateTime NotBeforeUtc { get; set; }
        public DateTime NotAfterUtc { get; set; }
        public bool IsCa { get; set; }
        public int? PathLenConstraint { get; set; }
        public string PemCert { get; set; } = default!;
        public string ChainPem { get; set; } = default!;
    }
}
