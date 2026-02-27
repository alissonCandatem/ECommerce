using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Produtos.Infrastructure
{
  public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
  {
    public AppDbContext CreateDbContext(string[] args)
    {
      var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
      optionsBuilder.UseNpgsql("Host=localhost;Port=5434;Database=ecommerce_produtos;Username=postgres;Password=postgres");

      return new AppDbContext(optionsBuilder.Options);
    }
  }
}
