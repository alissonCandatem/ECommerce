using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ECommerce.Pedidos.Api.Configuration
{
  public static class AuthConfiguration
  {
    public static IServiceCollection AddAuthConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)),
          ValidateIssuer = true,
          ValidIssuer = configuration["Jwt:Issuer"],
          ValidateAudience = true,
          ValidAudience = configuration["Jwt:Audience"],
          ValidateLifetime = true,
          ClockSkew = TimeSpan.Zero
        };
      });

      services.AddAuthorization();

      return services;
    }
  }
}
