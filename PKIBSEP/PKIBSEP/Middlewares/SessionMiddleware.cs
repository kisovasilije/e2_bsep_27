using PKIBSEP.Interfaces;

namespace PKIBSEP.Middlewares;

public class SessionMiddleware
{
    private readonly RequestDelegate next;

    //private readonly ISessionService service;

    private ILogger<SessionMiddleware> logger;

    public SessionMiddleware(RequestDelegate next, ILogger<SessionMiddleware> logger)
    {
        this.next = next;
        //this.service = service;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        var service = ctx.RequestServices.GetRequiredService<ISessionService>();

        var header = ctx.Request.Headers["Authorization"].ToString();
        var token = header.Substring("Bearer ".Length);

        if (!string.IsNullOrEmpty(token) && !token.Equals("null"))
        {
            var result = await service.ValidateSessionAsync(header.Substring("Bearer ".Length).Trim());
            if (result.IsFailed)
            {
                logger.LogWarning("Session validation failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Message)));

                ctx.Response.StatusCode = 401;
                ctx.Response.ContentType = "text/plain";
                await ctx.Response.WriteAsync("Session validation failed: " + string.Join(", ", result.Errors.Select(e => e.Message)));
                return;
            }

        }

        await next(ctx);
    }
}
