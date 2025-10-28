using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PKIBSEP.Dtos;
using PKIBSEP.Interfaces;

namespace PKIBSEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Returns CA users: Admin sees all, CA User sees only same organization
        /// </summary>
        [HttpGet("ca-users")]
        [Authorize(Policy = "adminOrCaPolicy")]
        public async Task<ActionResult<List<UserDto>>> GetCaUsers()
        {
            var currentUserId = GetCurrentUserId();
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var isAdmin = userRole == "Admin";

            List<UserDto> caUsers;
            if (isAdmin)
            {
                // Admin sees all CA users
                caUsers = await _userService.GetCaUsersAsync();
            }
            else
            {
                // CA User sees only CA users from same organization
                caUsers = await _userService.GetCaUsersByOrganizationAsync(currentUserId);
            }

            return Ok(caUsers);
        }

        private int GetCurrentUserId()
        {
            var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(sub, out var id))
                return id;
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        /// <summary>
        /// Registracija novog korisnika
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterAsync(registerDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Aktivacija naloga preko email linka
        /// </summary>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var (success, message) = await _userService.ConfirmEmailAsync(token);

            if (!success)
            {
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message });
        }

        /// <summary>
        /// Provera jačine lozinke
        /// </summary>
        [HttpGet("check-password-strength")]
        public ActionResult<PasswordStrengthDto> CheckPasswordStrength([FromQuery] string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return BadRequest(new { message = "Lozinka je obavezna" });
            }

            var strength = _userService.CheckPasswordStrength(password);
            return Ok(strength);
        }

        /// <summary>
        /// Zahtev za resetovanje lozinke (forgot password)
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message) = await _userService.ForgotPasswordAsync(forgotPasswordDto.Email);

            if (!success)
            {
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message });
        }

        /// <summary>
        /// Resetovanje lozinke preko linka iz emaila
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message) = await _userService.ResetPasswordAsync(
                resetPasswordDto.Token,
                resetPasswordDto.NewPassword);

            if (!success)
            {
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message });
        }
    }
}
