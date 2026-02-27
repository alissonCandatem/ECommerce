using Microsoft.OpenApi.Models;

namespace ECommerce.Produtos.Configuration
{
  public static class SwaggerConfiguration
  {
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
      services.AddEndpointsApiExplorer();
      services.AddSwaggerGen(options =>
      {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
          Name = "Authorization",
          Type = SecuritySchemeType.Http,
          Scheme = "Bearer",
          BearerFormat = "JWT",
          In = ParameterLocation.Header,
          Description = "Informe o token JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
          {
            new OpenApiSecurityScheme
            {
              Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              }
            },

            Array.Empty<string>()
          }
        });
      });

      return services;
    }
  }
}
