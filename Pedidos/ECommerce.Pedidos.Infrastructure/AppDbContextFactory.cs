using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Pedidos.Infrastructure
{
  public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
  {
    public AppDbContext CreateDbContext(string[] args)
    {
      var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
      optionsBuilder.UseNpgsql("Host=localhost;Port=5435;Database=ecommerce_pedidos;Username=postgres;Password=postgres");

      return new AppDbContext(optionsBuilder.Options);
    }
  }
}
