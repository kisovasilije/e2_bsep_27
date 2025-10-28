using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PKIBSEP.Dtos.Keys;
using PKIBSEP.Interfaces;
using System.Security.Claims;

namespace PKIBSEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "RegularUser")]
    public class KeyController : ControllerBase
    {
        private readonly IKeyService _keyService;
        private readonly ILogger<KeyController> _logger;

        public KeyController(IKeyService keyService, ILogger<KeyController> logger)
        {
            _keyService = keyService;
            _logger = logger;
        }

        /// <summary>
        /// Čuvanje javnog ključa za password manager
        /// </summary>
        [HttpPost("save-public-key")]
        public async Task<IActionResult> SavePublicKey([FromBody] SavePublicKeyDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new { message = "Korisnik nije autentifikovan" });
            }

            var (success, message) = await _keyService.SavePublicKeyAsync(userId, dto.PublicKeyPem);

            if (!success)
            {
                return BadRequest(new { success, message });
            }

            return Ok(new { success, message });
        }

        /// <summary>
        /// Preuzimanje sopstvenog javnog ključa
        /// </summary>
        [HttpGet("public-key")]
        public async Task<IActionResult> GetPublicKey()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new { message = "Korisnik nije autentifikovan" });
            }

            var result = await _keyService.GetPublicKeyAsync(userId);

            if (result == null)
            {
                return NotFound(new { message = "Korisnik nije pronađen" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Preuzimanje javnog ključa drugog korisnika (za deljenje lozinki)
        /// </summary>
        [HttpGet("public-key/{userId}")]
        public async Task<IActionResult> GetUserPublicKey([FromRoute] int userId)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var requestingUserId))
            {
                return Unauthorized(new { message = "Korisnik nije autentifikovan" });
            }

            var result = await _keyService.GetUserPublicKeyAsync(requestingUserId, userId);

            if (result == null)
            {
                return NotFound(new { message = "Korisnik nije pronađen ili nema javni ključ" });
            }

            return Ok(result);
        }
    }
}
