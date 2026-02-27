using ECommerce.Mediator;
using ECommerce.Mediator.Abstractions;
using ECommerce.Pedidos.Domain.Interfaces;
using ECommerce.Pedidos.Infrastructure.Outbox;
using ECommerce.Pedidos.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Pedidos.Infrastructure
{
  public static class Startup
  {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddDbContext<AppDbContext>(options =>
          options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

      services.AddScoped<IUnitOfWork, UnitOfWork>();
      services.AddScoped<IPedidoRepository, PedidoRepository>();
      services.AddScoped<IEventPublisher, InMemoryEventPublisher>();
      services.AddHostedService<OutboxProcessor>();

      return services;
    }
  }
}
