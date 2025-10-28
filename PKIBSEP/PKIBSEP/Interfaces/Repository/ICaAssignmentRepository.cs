namespace PKIBSEP.Interfaces.Repository
{
    /// <summary>
    /// Checks whether a CA user is allowed to use a given certificate chain (by chain root id).
    /// </summary>
    public interface ICaAssignmentRepository
    {
        /// <summary>
        /// Returns true if the CA user has an active assignment for the chain identified by <paramref name="chainRootCertificateId"/>.
        /// </summary>
        Task<bool> IsChainAssignedToUserAsync(int caUserId, int chainRootCertificateId);

        /// <summary>
        /// Creates a new CA assignment, linking a CA user to a certificate chain.
        /// </summary>
        Task<int> InsertAsync(Models.Certificate.CaAssignment assignment);

        /// <summary>
        /// Gets all active chain root certificate IDs assigned to a specific CA user.
        /// </summary>
        Task<List<int>> GetAssignedChainRootsForUserAsync(int caUserId);
    }
}
