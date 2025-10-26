using PKIBSEP.Common;
using PKIBSEP.Interfaces;

namespace PKIBSEP.Middlewares;

public class SessionMiddleware
{
    private readonly RequestDelegate next;

    private readonly ILogger<SessionMiddleware> logger;

    public SessionMiddleware(RequestDelegate next, ILogger<SessionMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        var service = ctx.RequestServices.GetRequiredService<ISessionService>();

        var token = ctx.Request.GetBearerToken();

        if (token != null)
        {
            var result = await service.ValidateSessionAsync(token);
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
