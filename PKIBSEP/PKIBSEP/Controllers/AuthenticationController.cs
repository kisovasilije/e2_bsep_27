using Microsoft.AspNetCore.Mvc;
using PKIBSEP.Dtos;
using PKIBSEP.Interfaces;

namespace PKIBSEP.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController:ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthenticationController(IAuthService authService) 
        { 
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserCredentialsDto userCredentials)
        {
            try
            {
                var result = await _authService.LoginAsync(userCredentials);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
