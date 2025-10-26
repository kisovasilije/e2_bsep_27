using PKIBSEP.Dtos.PasswordManager;

namespace PKIBSEP.Interfaces
{
    public interface IPasswordService
    {
        Task<List<PasswordEntryDto>> GetUserPasswordsAsync(int userId);
        Task<PasswordEntryDto?> GetPasswordByIdAsync(int id, int userId);
        Task<(bool success, string message, PasswordEntryDto? entry)> CreatePasswordAsync(int userId, SavePasswordDto dto);
        Task<(bool success, string message)> UpdatePasswordAsync(int userId, UpdatePasswordDto dto);
        Task<(bool success, string message)> DeletePasswordAsync(int id, int userId);
    }
}
