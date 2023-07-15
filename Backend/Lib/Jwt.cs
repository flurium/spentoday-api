using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Lib;

public record JwtSecrets(string Issuer, string Audience, string Secret);

public class Jwt
{
    public const string Uid = "uid";
    public const string Version = "version";

    private readonly JwtSecrets secrets;

    public Jwt(JwtSecrets secrets)
    {
        this.secrets = secrets;
    }

    public string Token(string uid, int version)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(Uid, uid),
            new Claim(Version, version.ToString())
        };

        var jwt = new JwtSecurityToken(
            issuer: secrets.Issuer,
            expires: DateTime.Now.AddDays(30),
            claims: claims,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public ClaimsPrincipal? Validate(string token)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.Secret));

            var validations = new TokenValidationParameters
            {
                ValidIssuer = secrets.Issuer,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateAudience = false, // for now
                ValidateLifetime = true,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, validations, out var _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}