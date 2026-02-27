using ECommerce.Pedidos.Domain.Entities;
using ECommerce.Pedidos.Infrastructure.OutBox;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Pedidos.Infrastructure
{
  public sealed class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
  }
}
