using PKIBSEP.Dtos;

namespace PKIBSEP.Interfaces
{
    public interface IAuthService
    {
        Task<AuthenticationResponseDto> LoginAsync(AuthenticationDto auth);
    }
}
