using PKIBSEP.Models;

namespace PKIBSEP.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailConfirmationTokenAsync(string token);
        Task<User?> GetByPasswordResetTokenAsync(string token);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> EmailExistsAsync(string email);
    }
}
