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
            if (!GetRequestMeta(out var ipAddress, out var userAgent))
            {
                return BadRequest("Error occurred getting request metadata.");
            }

            var auth = new AuthenticationDto(userCredentials.Email, userCredentials.Password, userCredentials.CaptchaToken, ipAddress, userAgent);

            try
            {
                var result = await _authService.LoginAsync(auth);
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
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private bool GetRequestMeta(out string ipAddress, out string userAgent)
        {
            ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            userAgent = Request.Headers["User-Agent"].ToString();
            if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrEmpty(userAgent))
            {
                return false;
            }

            return true;
        }
    }
}
