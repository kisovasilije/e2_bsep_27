namespace PKIBSEP.Dtos
{
    public class X500NameDto
    {
        /// <summary>Common Name (CN) – e.g., CA display name or server FQDN.</summary>
        public string CN { get; set; } = default!;
        /// <summary>Organization (O)</summary>
        public string? O { get; set; }
        /// <summary>Organizational Unit (OU)</summary>
        public string? OU { get; set; }
        /// <summary>Locality / City (L)</summary>
        public string? L { get; set; }
        /// <summary>State / Province (ST)</summary>
        public string? ST { get; set; }
        /// <summary>Country (C) — ISO 3166-1 alpha-2 (2 letters)</summary>
        public string? C { get; set; }
    }
}
