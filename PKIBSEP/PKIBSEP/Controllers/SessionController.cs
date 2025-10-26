using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PKIBSEP.Common;
using PKIBSEP.Interfaces;
using System.Runtime.CompilerServices;

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
        var token = Request.GetBearerToken();
        if (token is null)
        {
            return BadRequest("Authorization token is missing.");
        }

        var result = await sessionService.GetByUserIdAsync(userId, token);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpPatch("revoke-current-session")]
    public async Task<IActionResult> RevokeCurrentSession()
    {
        var token = Request.GetBearerToken();
        if (token is null)
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

    [HttpPatch("revoke/{id:int:min(1)}")]
    public async Task<IActionResult> RevokeSessionById([FromRoute] int id)
    {
        var result = await sessionService.RevokeByIdAsync(id);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }
}
