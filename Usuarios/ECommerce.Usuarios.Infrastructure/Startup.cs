using ECommerce.Mediator;
using ECommerce.Mediator.Abstractions;
using ECommerce.Usuarios.Application.Interfaces;
using ECommerce.Usuarios.Domain.Interfaces.Usuario;
using ECommerce.Usuarios.Infrastructure.Outbox;
using ECommerce.Usuarios.Infrastructure.Repositories;
using ECommerce.Usuarios.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Usuarios.Infrastructure
{
  public static class Startup
  {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(
            configuration.GetConnectionString("DefaultConnection")));

      services.AddScoped<IUnitOfWork, UnitOfWork>();
      services.AddScoped<IUsuarioRepository, UsuarioRepository>();
      services.AddScoped<IJwtService, JwtService>();

      // in-memory
      services.AddScoped<IEventPublisher, InMemoryEventPublisher>();

      // kafka
      services.AddScoped<IEventPublisher, KafkaEventPublisher>();

      // worker 
      services.AddHostedService<OutboxProcessor>();

      return services;
    }
  }
}
