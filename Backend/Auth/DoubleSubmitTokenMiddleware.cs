using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Auth;

/// <summary>
/// Require incomming request to have double submit cookie.
/// It's the same value in cookie and header.
/// It adds protection against CSRF attacks and doesn't require a lot of computations.
/// It must be used after UseRouting and before UseAuthentication.
/// Identifying whether endpoint requires authentication or not relies
/// on AuthorizeAttribute and AllowAnonymousAttribute. So you need to use them.
/// </summary>
public class DoubleSubmitTokenMiddleware
{
    private readonly RequestDelegate next;

    public DoubleSubmitTokenMiddleware(RequestDelegate next)
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

        var cookie = Cookie(context);
        if (cookie == null)
        {
            context.Response.StatusCode = 403;
            return;
        }

        var header = Header(context);
        if (header == null)
        {
            context.Response.StatusCode = 403;
            return;
        }

        if (cookie != header)
        {
            context.Response.StatusCode = 403;
            return;
        }

        await next(context);
    }

    private static bool RequiresAuthentication(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null || endpoint.Metadata.Count == 0) return false;

        for (int i = endpoint.Metadata.Count - 1; i >= 0; i -= 1)
        {
            var meta = endpoint.Metadata[i];
            if (meta == null) continue;

            if (meta.GetType() == typeof(AuthorizeAttribute)) return true;

            if (meta.GetType() == typeof(AllowAnonymousAttribute)) return false;
        }

        return false;
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

public static class DoubleSubmitTokenMiddlewareExtensions
{
    public static IApplicationBuilder UseDoubleSubmitToken(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DoubleSubmitTokenMiddleware>();
    }
}