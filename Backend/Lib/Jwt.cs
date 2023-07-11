using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Lib;

public record JwtSecrets(string Issuer, string Audience, string Secret);

public class Jwt
{
    private readonly JwtSecrets secrets;

    public Jwt(JwtSecrets secrets)
    {
        this.secrets = secrets;
    }

    public string Token(string uid)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, uid),
    };

        var jwt = new JwtSecurityToken(
            issuer: secrets.Issuer,
            expires: DateTime.Now.AddMinutes(15),
            claims: claims,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}