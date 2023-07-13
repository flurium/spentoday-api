using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Auth;

/// <summary>
/// Require incomming request to have double submit cookie.
/// It's the same value in cookie and header.
/// It adds protection against CSRF attacks and doesn't require a lot of computations.
/// I recommend to add the filter as global to all our endpoints.
/// </summary>
public class DoubleSubmitCookieFilter : IAuthorizationFilter
{
    private const string key = "double-submit";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var cookie = Cookie(context.HttpContext);
        if (cookie == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        var header = Header(context.HttpContext);
        if (header == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (cookie != header)
        {
            context.Result = new ForbidResult();
            return;
        }
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