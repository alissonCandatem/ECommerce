using ECommerce.Mediator;
using ECommerce.Mediator.Abstractions;
using ECommerce.Produtos.Domain.Interfaces;
using ECommerce.Produtos.Infrastructure.Kafka;
using ECommerce.Produtos.Infrastructure.Outbox;
using ECommerce.Produtos.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Produtos.Infrastructure
{
  public static class Startup
  {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddDbContext<AppDbContext>(options =>
          options.UseNpgsql(
              configuration.GetConnectionString("DefaultConnection")));

      services.AddScoped<IUnitOfWork, UnitOfWork>();
      services.AddScoped<IProdutoRepository, ProdutoRepository>();
      services.AddScoped<IEventPublisher, InMemoryEventPublisher>();
      services.AddHostedService<OutboxProcessor>();
      services.AddHostedService<PedidoCriadoConsumer>();

      return services;
    }
  }
}
