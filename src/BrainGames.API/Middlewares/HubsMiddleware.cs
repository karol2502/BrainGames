namespace BrainGames.API.Middlewares;

public class HubsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var request = httpContext.Request;

        // web sockets cannot pass headers so we must take the access token from query param and
        // add it to the header before authentication middleware runs
        if (request.Path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase) &&
            request.Query.TryGetValue("access_token", out var accessToken))
        {
            request.Headers.Append("Authorization", $"Bearer {accessToken}");
        }

        await next(httpContext);
    }
}