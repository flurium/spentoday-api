using Microsoft.AspNetCore.Authorization;

namespace Backend.Auth;

/// <summary>
/// Require custom header from request.
/// It adds protection against CSRF attacks and doesn't require a lot of computations.
/// It must be used after UseRouting and before UseAuthentication.
/// Identifying whether endpoint requires authentication or not relies
/// on AuthorizeAttribute. So you need to use it.
/// </summary>
public class CustomHeaderProtectionMiddleware
{
    private readonly RequestDelegate next;

    public CustomHeaderProtectionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    private const string key = "double-submit";

    public async Task InvokeAsync(HttpContext context)
    {
        var requiresAuthentication = RequiresAuthentication(context);

        if (!requiresAuthentication)
        {
            await next(context);
            return;
        }

        /*
        var cookie = Cookie(context);
        if (cookie == null)
        {
            context.Response.StatusCode = 403;
            return;
        }
        */

        var header = Header(context);
        if (header == null)
        {
            context.Response.StatusCode = 403;
            return;
        }

        /*
        if (cookie != header)
        {
            context.Response.StatusCode = 403;
            return;
        }
        */

        await next(context);
    }

    private static bool RequiresAuthentication(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null) return false;

        var authorize = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();
        return authorize != null;
    }

    private static string? Header(HttpContext context)
    {
        context.Request.Headers.TryGetValue(key, out var value);
        return value;
    }

    private static string? Cookie(HttpContext context)
    {
        context.Request.Cookies.TryGetValue(key, out var value);
        return value;
    }
}

public static class CustomHeaderProtectionExtensions
{
    public static IApplicationBuilder UseCustomHeaderProtection(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomHeaderProtectionMiddleware>();
    }
}