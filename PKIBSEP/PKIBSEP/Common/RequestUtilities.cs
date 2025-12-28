namespace PKIBSEP.Common;

public static class RequestUtilities
{
    public static string? GetBearerToken(this HttpRequest request)
    {
        var authHeader = request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader))
        {
            return null;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(authHeader) || token.Equals("null"))
        {
            return null;
        }

        return authHeader.Substring("Bearer ".Length).Trim();
    }
}
