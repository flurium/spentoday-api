using Lib;
using System.Security.Claims;

namespace Backend.Auth;

public static class AuthExtension
{
    /// <summary>
    /// Should be used only for [Authorize] routes.
    /// Otherwise throw error.
    /// </summary>
    /// <returns>Return user id from user claims principal.</returns>
    public static string Uid(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(Jwt.Uid)!.Value;
    }
}