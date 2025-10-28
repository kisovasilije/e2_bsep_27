namespace PKIBSEP.Interfaces
{
    public interface ICaUserKeyService
    {
        Task<byte[]> GetOrCreateWrapKeyAsync(int caUserId);
    }
}
