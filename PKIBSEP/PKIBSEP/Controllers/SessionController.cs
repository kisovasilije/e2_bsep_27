using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PKIBSEP.Interfaces;

namespace PKIBSEP.Controllers;

[Route("api/sessions")]
[ApiController]
public class SessionController : ControllerBase
{
    private readonly ISessionService sessionService;

    public SessionController(ISessionService sessionService)
    {
        this.sessionService = sessionService;
    }

    /// <summary>
    /// Gets all sessions for a specific user by user ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{userId:int:min(1)}")]
    public async Task<IActionResult> GetByUserId([FromRoute] int userId)
    {
        var result = await sessionService.GetByUserIdAsync(userId);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpPatch("revoke-current-session")]
    public async Task<IActionResult> RevokeCurrentSession()
    {
        var token = GetBearerToken();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Authorization token is missing.");
        }

        var result = await sessionService.RevokeCurrentSessionAsync(token);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }

    private string GetBearerToken()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return string.Empty;
        }

        return authHeader.Substring("Bearer ".Length).Trim();
    }
}
