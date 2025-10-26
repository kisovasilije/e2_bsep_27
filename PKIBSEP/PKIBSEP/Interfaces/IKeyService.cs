using PKIBSEP.Dtos.Keys;

namespace PKIBSEP.Interfaces
{
    public interface IKeyService
    {
        Task<(bool success, string message)> SavePublicKeyAsync(int userId, string publicKeyPem);
        Task<PublicKeyResponseDto?> GetPublicKeyAsync(int userId);
        Task<UserPublicKeyDto?> GetUserPublicKeyAsync(int requestingUserId, int targetUserId);
    }
}
