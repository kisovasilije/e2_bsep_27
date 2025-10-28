namespace PKIBSEP.Models.Certificate;

/// <summary>
/// Keystore record (.pfx) for a CA certificate:
/// - file path to PFX that contains private key + chain
/// - encrypted PFX password (AES-GCM), encrypted with per-CA-user wrap key
/// </summary>
public class CaKeyMaterial
{
    public int Id { get; set; }

    /// <summary>FK to the CA certificate this keystore belongs to.</summary>
    public int CertificateId { get; set; }

    /// <summary>
    /// CA User ID who owns this keystore (whose wrap key encrypts the PFX password).
    /// This allows Admin to use any CA certificate as issuer by loading the keystore
    /// with the correct owner's wrap key.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>Absolute file path to the .pfx keystore.</summary>
    public string PfxFilePath { get; set; } = default!;

    /// <summary>Encrypted PFX password (Base64: nonce|tag|ciphertext), via per-user wrap key.</summary>
    public string EncryptedPfxPassword { get; set; } = default!;

    /// <summary>Active flag (for rotations or deprecation).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>(Optional) Keystore alias, if needed for interop.</summary>
    public string? KeystoreAlias { get; set; }
}
