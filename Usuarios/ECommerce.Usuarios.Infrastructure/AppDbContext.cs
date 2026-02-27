using ECommerce.Usuarios.Domain.Entities.Usuario;
using ECommerce.Usuarios.Infrastructure.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Usuarios.Infrastructure
{
  public sealed class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
  }
}
