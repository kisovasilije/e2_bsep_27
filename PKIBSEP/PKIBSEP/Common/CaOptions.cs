namespace PKIBSEP.Common;

public sealed class CaOptions
{
    public string PrivateKeyPath { get; set; } = string.Empty;
    public string CertificatePath { get; set; } = string.Empty;
    public int DefaultValidityDays { get; set; } = 365;
}
