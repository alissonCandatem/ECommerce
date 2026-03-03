using ECommerce.Pedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Pedidos.Infrastructure.Mappings
{
  public sealed class PedidoItemConfiguration : IEntityTypeConfiguration<PedidoItem>
  {
    public void Configure(EntityTypeBuilder<PedidoItem> builder)
    {
      builder.ToTable("pedido_itens");

      builder.HasKey(x => x.Id);
      builder.Property(x => x.Id).HasColumnName("id");
      builder.Property(x => x.PedidoId).HasColumnName("pedido_id").IsRequired();
      builder.Property(x => x.ProdutoId).HasColumnName("produto_id").IsRequired();
      builder.Property(x => x.NomeProduto).HasColumnName("nome_produto").HasMaxLength(200).IsRequired();
      builder.Property(x => x.PrecoUnitario).HasColumnName("preco_unitario").HasPrecision(18, 2).IsRequired();
      builder.Property(x => x.Quantidade).HasColumnName("quantidade").IsRequired();
      builder.Ignore(x => x.Total);
    }
  }
}
