using ECommerce.Usuarios.Application.Interfaces;
using ECommerce.Usuarios.Domain.Entities.Usuario;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Usuarios.Infrastructure.Services
{
  public sealed class JwtService : IJwtService
  {
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiresInMinutes;

    public JwtService(IConfiguration configuration)
    {
      _secretKey = configuration["Jwt:SecretKey"]!;
      _issuer = configuration["Jwt:Issuer"]!;
      _audience = configuration["Jwt:Audience"]!;
      _expiresInMinutes = int.Parse(configuration["Jwt:ExpiresInMinutes"]!);
    }

    public string GerarAccessToken(User user)
    {
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var claims = new List<Claim>
      {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email),
        new(ClaimTypes.Name, user.Nome),
        new(ClaimTypes.Role, user.Role.ToString()),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
      };

      var token = new JwtSecurityToken(
        issuer: _issuer,
        audience: _audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(_expiresInMinutes),
        signingCredentials: credentials
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GerarRefreshToken()
    {
      var bytes = RandomNumberGenerator.GetBytes(64);
      return Convert.ToBase64String(bytes);
    }

    public Guid? ObterUserIdToken(string accessToken)
    {
      try
      {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

        handler.ValidateToken(accessToken, new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = key,
          ValidateIssuer = true,
          ValidIssuer = _issuer,
          ValidateAudience = true,
          ValidAudience = _audience,
          ValidateLifetime = false
        }, out var validatedToken);

        var jwt = (JwtSecurityToken)validatedToken;

        var userId = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        return userId != null ? Guid.Parse(userId) : null;
      }
      catch
      {
        return null;
      }
    }
  }
}
