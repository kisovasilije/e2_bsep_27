using FluentResults;
using PKIBSEP.Dtos;

namespace PKIBSEP.Interfaces;

public interface ISessionService
{
    Task<Result> CreateAsync(AuthenticationDto auth);
}
