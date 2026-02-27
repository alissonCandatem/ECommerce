using ECommerce.Usuarios.Domain.Entities.Usuario;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Usuarios.Infrastructure.Mappings
{
  public sealed class UsuarioMap : IEntityTypeConfiguration<User>
  {
    public void Configure(EntityTypeBuilder<User> builder)
    {
      builder.ToTable("users");

      builder.HasKey(x => x.Id);

      builder.Property(x => x.Id)
          .HasColumnName("id");

      builder.Property(x => x.Nome)
          .HasColumnName("nome")
          .HasMaxLength(100)
          .IsRequired();

      builder.Property(x => x.Email)
          .HasColumnName("email")
          .HasMaxLength(200)
          .IsRequired();

      builder.HasIndex(x => x.Email)
          .IsUnique();

      builder.Property(x => x.SenhaHash)
          .HasColumnName("senha_hash")
          .IsRequired();

      builder.Property(x => x.Role)
          .HasColumnName("role")
          .HasConversion<int>()
          .IsRequired();

      builder.Property(x => x.RefreshToken)
          .HasColumnName("refresh_token");

      builder.Property(x => x.RefreshTokenExpiry)
          .HasColumnName("refresh_token_expiry");

      builder.Property(x => x.CriadoEm)
          .HasColumnName("criado_em")
          .IsRequired();
    }
  }
}
