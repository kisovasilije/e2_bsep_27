using PKIBSEP.Models;

namespace PKIBSEP.Interfaces.Repository
{
    public interface IPasswordRepository
    {
        Task<List<PasswordEntry>> GetUserPasswordsAsync(int userId);
        Task<PasswordEntry?> GetByIdAsync(int id);
        Task<PasswordEntry> CreateAsync(PasswordEntry entry);
        Task UpdateAsync(PasswordEntry entry);
        Task DeleteAsync(int id);
        Task<PasswordShare?> GetPasswordShareAsync(int entryId, int userId);
        Task<PasswordShare> AddPasswordShareAsync(PasswordShare share);
        Task<List<PasswordShare>> GetPasswordSharesAsync(int entryId);
    }
}
