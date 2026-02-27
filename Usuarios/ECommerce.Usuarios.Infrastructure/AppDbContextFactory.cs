using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Usuarios.Infrastructure
{
  public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
  {
    public AppDbContext CreateDbContext(string[] args)
    {
      var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

      optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=ecommerce_usuarios;Username=postgres;Password=postgres");

      return new AppDbContext(optionsBuilder.Options);
    }
  }
}
