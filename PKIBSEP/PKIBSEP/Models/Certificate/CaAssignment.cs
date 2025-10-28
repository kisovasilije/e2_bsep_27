namespace PKIBSEP.Models.Certificate
{
    /// <summary>
    /// Assigns a chain (by root id) to a CA user. CA users can issue only inside assigned chains.
    /// </summary>
    public class CaAssignment
    {
        public int Id { get; set; }
        public int CaUserId { get; set; }
        public int ChainRootCertificateId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? AssignedByUserId { get; set; }
        public string? Organization { get; set; }
    }
}
