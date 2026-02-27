using ECommerce.Produtos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Produtos.Infrastructure.Mappings
{
  public sealed class ProdutoMap : IEntityTypeConfiguration<Produto>
  {
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
      builder.ToTable("produtos");

      builder.HasKey(x => x.Id);

      builder.Property(x => x.Id)
          .HasColumnName("id");

      builder.Property(x => x.Nome)
          .HasColumnName("nome")
          .HasMaxLength(200)
          .IsRequired();

      builder.Property(x => x.Descricao)
          .HasColumnName("descricao")
          .HasMaxLength(1000)
          .IsRequired();

      builder.Property(x => x.Preco)
          .HasColumnName("preco")
          .HasPrecision(18, 2)
          .IsRequired();

      builder.Property(x => x.Estoque)
          .HasColumnName("estoque")
          .IsRequired();

      builder.Property(x => x.Categoria)
          .HasColumnName("categoria")
          .HasMaxLength(100)
          .IsRequired();

      builder.Property(x => x.Ativo)
          .HasColumnName("ativo")
          .IsRequired();

      builder.Property(x => x.CriadoEm)
          .HasColumnName("criado_em")
          .IsRequired();
    }
  }
}
