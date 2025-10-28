using Org.BouncyCastle.Asn1.X509;
using PKIBSEP.Dtos;

namespace PKIBSEP.Crypto.X509
{
    /// <summary>
    /// Maps KeyUsageDto to BouncyCastle KeyUsage flags.
    /// </summary>
    public static class KeyUsageMapper
    {
        /// <summary>
        /// Converts KeyUsageDto to BouncyCastle KeyUsage integer flags.
        /// If dto is null, returns default CA flags (KeyCertSign | CrlSign).
        /// </summary>
        public static int ToBouncyCastleFlags(KeyUsageDto? dto)
        {
            // Default for CA certificates
            if (dto == null)
                return KeyUsage.KeyCertSign | KeyUsage.CrlSign;

            int flags = 0;

            if (dto.DigitalSignature) flags |= KeyUsage.DigitalSignature;
            if (dto.NonRepudiation) flags |= KeyUsage.NonRepudiation;
            if (dto.KeyEncipherment) flags |= KeyUsage.KeyEncipherment;
            if (dto.DataEncipherment) flags |= KeyUsage.DataEncipherment;
            if (dto.KeyAgreement) flags |= KeyUsage.KeyAgreement;
            if (dto.KeyCertSign) flags |= KeyUsage.KeyCertSign;
            if (dto.CrlSign) flags |= KeyUsage.CrlSign;
            if (dto.EncipherOnly) flags |= KeyUsage.EncipherOnly;
            if (dto.DecipherOnly) flags |= KeyUsage.DecipherOnly;

            // Safety check: if no flags set, use CA defaults
            if (flags == 0)
                return KeyUsage.KeyCertSign | KeyUsage.CrlSign;

            return flags;
        }
    }
}
