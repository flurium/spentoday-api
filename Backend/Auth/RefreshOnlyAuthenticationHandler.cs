using Backend.Lib;
using Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Text.Encodings.Web;

namespace Backend.Auth;

public static class RefreshOnly
{
    public const string Cookie = "refresh-only";
    public const string Scheme = "RefreshOnly";
}

public class RefreshOnlyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string bearer = "Bearer ";
    private readonly Jwt jwt;
    private readonly Db db;

    public RefreshOnlyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, ISystemClock clock, Jwt jwt, Db db
    ) : base(options, logger, encoder, clock)
    {
        this.jwt = jwt;
        this.db = db;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var token = GetToken(Request);
            if (token == null) return AuthenticateResult.Fail("Missing token");

            var princimal = jwt.Validate(token);
            if (princimal == null) return AuthenticateResult.Fail("Invalid token");

            var uid = princimal.FindFirst(Jwt.Uid)?.Value;
            if (uid == null) return AuthenticateResult.Fail("Missing user id");

            var versionParsed = int.TryParse(princimal.FindFirst(Jwt.Version)?.Value, out var version);
            if (!versionParsed) return AuthenticateResult.Fail("Missing version");

            var versionOk = await db.Users.AnyAsync(x => x.Id == uid && x.Version == version).ConfigureAwait(false);
            if (!versionOk) return AuthenticateResult.Fail("Invalid claims");

            // return user claims
            var ticket = new AuthenticationTicket(princimal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch
        {
            return AuthenticateResult.Fail("Unexpected error");
        }
    }

    /// <summary>
    /// Get token refresh only from request. Priority:
    /// 1. Cookie
    /// 2. Authorization Header
    /// </summary>
    private static string? GetToken(HttpRequest request)
    {
        request.Cookies.TryGetValue(RefreshOnly.Cookie, out var cookie);
        if (cookie != null) return cookie;

        if (StringValues.IsNullOrEmpty(request.Headers.Authorization)) return null;

        var header = request.Headers.Authorization.ToString();
        if (!header.StartsWith(bearer)) return null;

        var token = header.Substring(bearer.Length).Trim();
        if (string.IsNullOrEmpty(token)) return null;

        return token;
    }
}