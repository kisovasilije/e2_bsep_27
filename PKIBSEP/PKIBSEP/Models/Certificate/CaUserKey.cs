namespace Pki.Domain;

/// <summary>
/// Per-CA-user symmetric wrap key (32 bytes) used to encrypt PFX passwords.
/// The wrap key is itself protected using ASP.NET Data Protection (or OS keychain).
/// </summary>
public class CaUserKey
{
    public int Id { get; set; }

    /// <summary>CA user's id from your auth domain (Guid or int; use your type).</summary>
    public int CaUserId { get; set; }

    /// <summary>Protected wrap key (Base64) – DataProtection.Protect(rawKeyBytes).</summary>
    public string ProtectedWrapKey { get; set; } = default!;

    public int KeyVersion { get; set; } = 1;

    public DateTime CreatedAtUtc { get; set; }
}
