using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PKIBSEP.Dtos.PasswordManager;
using PKIBSEP.Interfaces;
using System.Security.Claims;

namespace PKIBSEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordService _passwordService;
        private readonly ILogger<PasswordController> _logger;

        public PasswordController(IPasswordService passwordService, ILogger<PasswordController> logger)
        {
            _passwordService = passwordService;
            _logger = logger;
        }

        /// <summary>
        /// Preuzimanje svih lozinki korisnika
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPasswords()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new { message = "Korisnik nije autentifikovan" });
            }

            var passwords = await _passwordService.GetUserPasswordsAsync(userId);
            return Ok(passwords);
        }

        /// <summary>
        /// Preuzimanje jedne lozinke po ID-u
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPasswordById([FromRoute] int id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new { message = "Korisnik nije autentifikovan" });
            }

            var password = await _passwordService.GetPasswordByIdAsync(id, userId);

            if (password == null)
            {
                return NotFound(new { message = "Lozinka nije pronađena ili nemate pristup" });
            }

            return Ok(password);
        }

        /// <summary>
        /// Kreiranje nove lozinke
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePassword([FromBody] SavePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new { message = "Korisnik nije autentifikovan" });
            }

            var (success, message, entry) = await _passwordService.CreatePasswordAsync(userId, dto);

            if (!success)
            {
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message, entry });
        }

        /// <summary>
        /// Ažuriranje postojeće lozinke
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePassword([FromRoute] int id, [FromBody] UpdatePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new { message = "Korisnik nije autentifikovan" });
            }

            dto.Id = id; // Ensure ID from route matches DTO

            var (success, message) = await _passwordService.UpdatePasswordAsync(userId, dto);

            if (!success)
            {
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message });
        }

        /// <summary>
        /// Brisanje lozinke
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePassword([FromRoute] int id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new { message = "Korisnik nije autentifikovan" });
            }

            var (success, message) = await _passwordService.DeletePasswordAsync(id, userId);

            if (!success)
            {
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message });
        }

        /// <summary>
        /// Preuzimanje liste dostupnih korisnika za deljenje lozinke
        /// </summary>
        [HttpGet("{id}/available-users")]
        public async Task<IActionResult> GetAvailableUsersForSharing([FromRoute] int id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new { message = "Korisnik nije autentifikovan" });
            }

            var users = await _passwordService.GetAvailableUsersForSharingAsync(userId, id);
            return Ok(users);
        }

        /// <summary>
        /// Deljenje lozinke sa drugim korisnikom
        /// </summary>
        [HttpPost("{id}/share")]
        public async Task<IActionResult> SharePassword([FromRoute] int id, [FromBody] SharePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new { message = "Korisnik nije autentifikovan" });
            }

            var (success, message) = await _passwordService.SharePasswordAsync(userId, id, dto);

            if (!success)
            {
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message });
        }

        /// <summary>
        /// Preuzimanje liste korisnika sa kojima je lozinka podeljena
        /// </summary>
        [HttpGet("{id}/shares")]
        public async Task<IActionResult> GetPasswordShares([FromRoute] int id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new { message = "Korisnik nije autentifikovan" });
            }

            var shares = await _passwordService.GetPasswordSharesAsync(id, userId);
            return Ok(shares);
        }
    }
}
