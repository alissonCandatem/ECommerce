using ECommerce.Produtos.Domain.Entities;
using ECommerce.Produtos.Infrastructure.OutBox;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Produtos.Infrastructure
{
  public sealed class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
  }
}
