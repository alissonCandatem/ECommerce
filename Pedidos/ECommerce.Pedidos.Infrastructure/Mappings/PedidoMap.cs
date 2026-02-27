using ECommerce.Pedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Pedidos.Infrastructure.Mappings
{
  public sealed class PedidoMap : IEntityTypeConfiguration<Pedido>
  {
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
      builder.ToTable("pedidos");

      builder.HasKey(x => x.Id);

      builder.Property(x => x.Id)
          .HasColumnName("id");

      builder.Property(x => x.UsuarioId)
          .HasColumnName("usuario_id")
          .IsRequired();

      builder.Property(x => x.Status)
          .HasColumnName("status")
          .HasConversion<int>()
          .IsRequired();

      builder.Property(x => x.CriadoEm)
          .HasColumnName("criado_em")
          .IsRequired();

      builder.Property(x => x.AtualizadoEm)
          .HasColumnName("atualizado_em");

      builder.HasMany(x => x.Itens)
          .WithOne()
          .HasForeignKey(x => x.PedidoId)
          .OnDelete(DeleteBehavior.Cascade);

      builder.Navigation(x => x.Itens).AutoInclude();
    }
  }
}
