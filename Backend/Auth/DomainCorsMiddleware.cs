using Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Auth;

/// <summary>
/// Replace CORS origin checking, because it can't be async.
/// This middleware will check if domain of request exists in shop domains.
/// If not then request will be rejected.
/// </summary>
public class DomainCorsMiddleware
{
    private readonly RequestDelegate next;
    private readonly Db db;

    public DomainCorsMiddleware(RequestDelegate next, Db db)
    {
        this.next = next;
        this.db = db;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var domain = context.Request.Host.Host;

        if (domain.EndsWith("spentoday.com") || domain.EndsWith("flurium.com"))
        {
            await next(context);
            return;
        }

        // maybe change in future
        var domainAllowed = await db.ShopDomains.AnyAsync(x => x.Domain == domain);
        if (!domainAllowed)
        {
            context.Response.StatusCode = 403;
            return;
        }

        await next(context);
    }
}

public static class DomainMiddlewareExtensions
{
    public static IApplicationBuilder UseDomainCors(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DomainCorsMiddleware>();
    }
}