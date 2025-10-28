namespace PKIBSEP.Dtos
{
    /// <summary>
    /// Defines which key usage flags should be set in the certificate.
    /// For CA certificates, typically include KeyCertSign and CrlSign.
    /// </summary>
    public class KeyUsageDto
    {
        /// <summary>Digital signatures (0x01)</summary>
        public bool DigitalSignature { get; set; }

        /// <summary>Non-repudiation (0x02)</summary>
        public bool NonRepudiation { get; set; }

        /// <summary>Key encipherment (0x04)</summary>
        public bool KeyEncipherment { get; set; }

        /// <summary>Data encipherment (0x08)</summary>
        public bool DataEncipherment { get; set; }

        /// <summary>Key agreement (0x10)</summary>
        public bool KeyAgreement { get; set; }

        /// <summary>Certificate signing (0x20) - required for CA certificates</summary>
        public bool KeyCertSign { get; set; }

        /// <summary>CRL signing (0x40) - typically set for CA certificates</summary>
        public bool CrlSign { get; set; }

        /// <summary>Encipher only (0x80)</summary>
        public bool EncipherOnly { get; set; }

        /// <summary>Decipher only (0x100)</summary>
        public bool DecipherOnly { get; set; }
    }
}
