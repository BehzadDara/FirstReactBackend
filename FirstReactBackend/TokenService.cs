using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FirstReactBackend;

public class TokenService
{
    public string Generate(int id)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, id.ToString()),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("FirsReactBackendBEHZADDARAFirsReactBackendBEHZADDARAFirsReactBackendBEHZADDARA"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var now = DateTime.Now;
        var tokenDescriptor = new JwtSecurityToken(
            "http://localhost:28747/",
            "http://localhost:28747/",
            claims,
            now,
            now.AddYears(10),
            credentials);

        var token = tokenHandler.WriteToken(tokenDescriptor);
        return token;
    }
}
