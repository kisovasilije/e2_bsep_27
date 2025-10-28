using PKIBSEP.Dtos;

namespace PKIBSEP.Interfaces
{
    public interface IUserService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<(bool success, string message)> ConfirmEmailAsync(string token);
        Task<(bool success, string message)> ForgotPasswordAsync(string email);
        Task<(bool success, string message)> ResetPasswordAsync(string token, string newPassword);
        PasswordStrengthDto CheckPasswordStrength(string password);
        Task<List<UserDto>> GetCaUsersAsync();
        Task<List<UserDto>> GetCaUsersByOrganizationAsync(int userId);
    }
}
