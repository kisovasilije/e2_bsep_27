using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using PKIBSEP.Interfaces;

namespace PKIBSEP.Controllers;

[Route("api/ocsp")]
[ApiController]
public class OcspController : ControllerBase
{
    private readonly ICaService caService;
    private readonly ILogger<OcspController> logger;

    public OcspController(ICaService caService, ILogger<OcspController> logger)
    {
        this.caService = caService;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        if (!Request.ContentType?.Equals("application/ocsp-request") ?? true)
        {
            return StatusCode(415);
        }

        byte[] ocspRequestBytes;
        using var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms);
        ocspRequestBytes = ms.ToArray();

        if (ocspRequestBytes.Length <= 0)
        {
            return BadRequest("Empty request body.");
        }

        var ocspResponseBytes = await caService.GetOcspResponseAsync(ocspRequestBytes);
        return File(
            ocspResponseBytes,
            "application/ocsp-response");
    }
}
